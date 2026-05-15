# app/agents/subgraphs/product.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage
from psycopg2.extras import RealDictCursor

# Import from core
from app.agents.state import MasterState, ProductSearchAction, Product, ProductUIResponse
from app.agents.config import llm, get_db_connection

from pydantic import BaseModel, Field
from typing import List

class UIResponse(BaseModel):
    text: str = Field(description="Natural language response from the AI (concise)")
    product_ids: List[int] = Field(default_factory=list, description="List of product IDs found in the database results")

def product_search(state: MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Product search Agent ---")

    chat_history = [
        msg for msg in state["messages"] 
        if isinstance(msg, (HumanMessage, AIMessage))
    ]
    recent_history = chat_history[-5:] 
    
    history_text = "\n".join([
        f"{'User' if isinstance(msg, HumanMessage) else 'AI'}: {msg.content}" 
        for msg in recent_history[:-1] 
    ])
    
    if not history_text:
        history_text = "This is the start of the conversation."

    product_llm = llm.with_structured_output(Product)
    prompt = f"""
        You are a product query parser for an e-commerce platform.
        Your ONLY job is to extract structured product search fields from the user's latest message.

        CONVERSATION HISTORY (most recent last):
        {history_text}

        ═══════════════════════════════════════════
        OUTPUT SCHEMA
        ═══════════════════════════════════════════
        name  : The core product name (noun only, no adjectives, no brand unless user specifies)
        des   : All descriptive attributes — color, material, style, brand, size, use-case, etc.
                Combine context + latest message. Use "none" if no description.
        price : A tuple of (operator, amount)
                Operators:
                "less"    → user said "under", "below", "cheaper than", "at most", "max"
                "equal"   → user said "exactly", "is it", "costs"
                "greater" → user said "above", "over", "at least", "more than"
                "ask"     → user is asking for the price (e.g. "how much is...?")
                "unknown" → no price info mentioned
                Amount: integer in USD. Use 0 when operator is "unknown" or "ask".

        ═══════════════════════════════════════════
        CONTEXT INHERITANCE RULE
        ═══════════════════════════════════════════
        If the latest message is a follow-up or refinement (e.g., "what about blue?",
        "cheaper ones?", "in size M?"), resolve the BASE PRODUCT from conversation history
        and MERGE it with the new attributes from the latest message.

        Example:
        History:  User: "Show me waterproof jackets"
        Latest:   "Do you have them under $80 in black?"
        → name: "jacket", des: "waterproof, black", price: ("less", 80)  ✓
        → name: "them", des: "black", price: ("less", 80)                ✗

        ═══════════════════════════════════════════
        EXTRACTION EXAMPLES
        ═══════════════════════════════════════════
        "Do you have any red shirts"
        → name: "shirt",            des: "red",                   price: ("unknown", 0)

        "waterproof jacket less than 50 usd"
        → name: "jacket",           des: "waterproof",            price: ("less", 50)

        "How much is the Minimal Logo Tee?"
        → name: "Minimal Logo Tee", des: "Minimal Logo",          price: ("ask", 0)

        "Nike running shoes above $120"
        → name: "shoes",            des: "Nike, running",         price: ("greater", 120)

        "something casual for summer under $40"
        → name: "clothing",         des: "casual, summer",        price: ("less", 40)

        "Do you have gaming keyboards?"
        → name: "keyboard",         des: "gaming",                price: ("unknown", 0)

        [History: User asked for hoodies → AI listed hoodies]
        "how about in white, size L?"
        → name: "hoodie",           des: "white, size L",         price: ("unknown", 0)

        ═══════════════════════════════════════════
        EDGE CASE RULES
        ═══════════════════════════════════════════
        - Brand names go into `des`, not `name`.           Nike shoes → name: "shoes", des: "Nike"
        - If name is truly unresolvable, use "unknown".
        - Never include adjectives in `name`.              "blue jacket" → name: "jacket", des: "blue"
        - Ranges like "$50-$100": pick the upper bound with operator "less". → ("less", 100)
        - Vague queries like "something cheap": name: "unknown", des: "cheap", price: ("unknown", 0)
        """
    
    response = product_llm.invoke([
        SystemMessage(content=prompt),
        HumanMessage(content=state['user_prompt']),
    ])
    
    action = ProductSearchAction().set_product(response)
    print(f"Product Context Extraction: {response}")
    
    return {"log_action": [action]}

def generate_product_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    product_model = action.get_product() if isinstance(action, ProductSearchAction) else None

    system_prompt = f"""
You are a PostgreSQL query generation engine for an e-commerce platform.
Your ONLY output is a single, executable SQL query — no markdown, no explanation, no backticks.

═══════════════════════════════════════════
DATABASE SCHEMA
═══════════════════════════════════════════
TABLE: products (p)
  - product_id    : UUID, PRIMARY KEY
  - product_name  : TEXT
  - description   : TEXT
  - status        : INTEGER  → 0=draft | 1=active | 2=inactive | 3=removed | 4=archived

TABLE: product_skus (ps)
  - sku_id        : UUID, PRIMARY KEY
  - product_id    : UUID, FK → products
  - price         : NUMERIC

TABLE: inventory (i)
  - sku_id        : UUID, FK → product_skus

JOINS (always use these):
  products p
  JOIN product_skus ps ON p.product_id = ps.product_id
  JOIN inventory i     ON ps.sku_id    = i.sku_id

═══════════════════════════════════════════
MANDATORY RULES (never violate)
═══════════════════════════════════════════
[R1] ALWAYS SELECT p.product_id — required by frontend.
[R2] ALWAYS filter p.status = 1 — active products only.
[R3] ALWAYS end with ORDER BY SIMILARITY(...) DESC LIMIT 5.
[R4] Output PLAIN SQL TEXT only — no markdown, no code fences, no comments.

═══════════════════════════════════════════
QUERY CONSTRUCTION GUIDE
═══════════════════════════════════════════

SELECT clause (always include):
  p.product_id, p.product_name, ps.price, p.description, p.status

WHERE clause — build with these blocks:

  ┌─ NAME BLOCK (always include) ───────────────────────────────────────┐
  │ (p.product_name ILIKE '%<name>%' OR p.product_name ILIKE '%<des>%') │
  └─────────────────────────────────────────────────────────────────────┘

  ┌─ PRICE BLOCK (conditional) ──────────────────────────────────────────┐
  │ operator = "less"    → AND ps.price < <amount>                       │
  │ operator = "equal"   → AND ps.price = <amount>                       │
  │ operator = "greater" → AND ps.price > <amount>                       │
  │ operator = "ask"     → OMIT price condition (user wants to see it)   │
  │ operator = "unknown" → OMIT price condition                          │
  └──────────────────────────────────────────────────────────────────────┘

  ┌─ DESCRIPTION BLOCK (always include, always OR) ──────────────────────┐
  │ OR (                                                                  │
  │   p.description ILIKE '%<keyword_1>%'                                 │
  │   OR p.description ILIKE '%<keyword_2>%'                              │
  │   OR p.description ILIKE '%<semantic_synonym>%'                       │
  │   ...expand with semantically related terms...                        │
  │ )                                                                     │
  └──────────────────────────────────────────────────────────────────────┘

  ┌─ STATUS BLOCK (always include) ──────────────────────────────────────┐
  │ AND p.status = 1                                                      │
  └──────────────────────────────────────────────────────────────────────┘

ORDER BY clause (always include):
  ORDER BY SIMILARITY(p.product_name, '<name> <des>') DESC LIMIT 5

═══════════════════════════════════════════
EXAMPLES
═══════════════════════════════════════════

INPUT:  product=jacket, des=waterproof, price=("equal", 100)
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
JOIN inventory i     ON ps.sku_id    = i.sku_id
WHERE (p.product_name ILIKE '%jacket%' OR p.product_name ILIKE '%waterproof%')
  AND ps.price = 100
  AND p.status = 1
  OR (
    p.description ILIKE '%waterproof%'
    OR p.description ILIKE '%jacket%'
    OR p.description ILIKE '%rain%'
    OR p.description ILIKE '%windbreaker%'
    OR p.description ILIKE '%weather resistant%'
  )
ORDER BY SIMILARITY(p.product_name, 'waterproof jacket') DESC LIMIT 5

---

INPUT:  product=shirt, des=red, price=("unknown", 0)
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
JOIN inventory i     ON ps.sku_id    = i.sku_id
WHERE (p.product_name ILIKE '%shirt%' OR p.product_name ILIKE '%red%')
  AND p.status = 1
  OR (
    p.description ILIKE '%shirt%'
    OR p.description ILIKE '%red%'
    OR p.description ILIKE '%crimson%'
    OR p.description ILIKE '%scarlet%'
  )
ORDER BY SIMILARITY(p.product_name, 'red shirt') DESC LIMIT 5

---

INPUT:  product=Minimal Logo Tee, des=Minimal Logo, price=("ask", 0)
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
JOIN inventory i     ON ps.sku_id    = i.sku_id
WHERE (p.product_name ILIKE '%Minimal Logo Tee%' OR p.product_name ILIKE '%Minimal Logo%')
  AND p.status = 1
  OR (
    p.description ILIKE '%minimal%'
    OR p.description ILIKE '%logo%'
    OR p.description ILIKE '%tee%'
    OR p.description ILIKE '%graphic tee%'
  )
ORDER BY SIMILARITY(p.product_name, 'Minimal Logo Tee') DESC LIMIT 5
"""

    convert_prompt = f"""
Generate a SQL query for the following product search:

Product name : {product_model.name}
Description  : {product_model.des}
Price filter : operator={product_model.price[0]}, amount={product_model.price[1]} VND
"""
    
    result = llm.invoke([
        SystemMessage(content=system_prompt),
        HumanMessage(content=convert_prompt)
    ])
    
    if isinstance(action, ProductSearchAction):
        action.set_query(result.content.strip())
        
    print(f"Product SQL Query: {result.content}")
    return {}

def execute_product_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    query = action.get_query() if isinstance(action, ProductSearchAction) else ""
    
    conn = get_db_connection()
    if not conn:
        if isinstance(action, ProductSearchAction): action.set_product_res([{"error": "DB Connection Failed"}])
        return {}
    
    try:
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            if not query.lower().startswith(("select", "with")):
                raise ValueError("Only SELECT queries allowed")
            
            cur.execute(query)
            clean_results = [dict(row) for row in cur.fetchall()] if cur.description else [{"status": "Executed (no data)"}]
            
            if isinstance(action, ProductSearchAction):    
                action.set_product_res(clean_results)
            conn.commit()
            
    except Exception as e:
        print(f"SQL Error: {e}")
        if isinstance(action, ProductSearchAction): action.set_product_res([{"error": str(e)}])
    finally:
        if conn: conn.close()
        
    return {}
    
def synthesize_product_answer(state: MasterState):
    action = state["log_action"][-1]
    results = action.get_product_res()

    # If DB has an error or found nothing
    if not results or "error" in results[0] or "Executed (no data)" in str(results[0].get("status", "")):
        fallback_msg = "I couldn't find any products matching your request at the moment. Please try different keywords!"
        return {
            "answer": fallback_msg,                    # Trả đúng kiểu string
            "ui_data": {"product_ids": []},            # Dữ liệu UI trống
            "messages": [AIMessage(content=fallback_msg)]
        }

    # If data exists, use Structured Output to create the MCP JSON
    ui_llm = llm.with_structured_output(UIResponse)
    
    system_prompt = system_prompt = """You are a helpful e-commerce assistant.
    You receive data from the database containing products matching the user's request.
    
    YOUR TASK:
    1. Consider the database results and user query. Only consider the top 5 results that match the user's intent. If there are more than 5 results of none of them match, return an empty list.
    2. Give answer base on the data, but keep it concise and natural. Don't just read out the product names — synthesize a helpful response. For example, if the user asked for "red shirts under $50" and you found 3 matching products, you might say: "I found 3 red shirts under $50. The 'Crimson Tee' is $30, the 'Scarlet Blouse' is $45, and the 'Ruby Polo' is $25. Would you like to see more details on any of these?"
    3. Extract all the `product_id` values from the database results and return them as a list of integers in the `product_ids` field.
    """
    
    response_model = ui_llm.invoke([
        SystemMessage(content=system_prompt + f"Data Results: {results}"),
        HumanMessage(content=state["user_prompt"])
    ])
    
    # 3. Trả về rành mạch từng field vào MasterState
    return {
        "answer": response_model.text,
        "ui_data": {"product_ids": response_model.product_ids},
        "messages": [AIMessage(content=response_model.text)]
    }
    

# ==========================================
# BUILD SUBGRAPH
# ==========================================
product_workflow = StateGraph(MasterState)
product_workflow.add_node("product_search", product_search)
product_workflow.add_node("generate_product_query", generate_product_query)
product_workflow.add_node("execute_product_query", execute_product_query)
product_workflow.add_node("synthesize_product_answer", synthesize_product_answer)

product_workflow.add_edge(START, "product_search")
product_workflow.add_edge("product_search", "generate_product_query")
product_workflow.add_edge("generate_product_query", "execute_product_query")
product_workflow.add_edge("execute_product_query", "synthesize_product_answer")
product_workflow.add_edge("synthesize_product_answer", END)

product_graph_app = product_workflow.compile()