# app/agents/subgraphs/product.py
import time

from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage,ToolMessage
from psycopg2.extras import RealDictCursor

# Import from core
from app.agents.state import MasterState, ProductSearchAction, Product
from app.agents.config import llm, get_db_connection

from pydantic import BaseModel, Field
from typing import List

class UIResponse(BaseModel):
    answer: str = Field(description="Natural language response from the AI (concise)")
    product_ids: List[int] = Field(default_factory=list, description="List of product IDs found in the database results")


def product_search(state: MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Product search Agent ---")

    chat_history = [
        msg for msg in state["messages"] 
        if isinstance(msg, (HumanMessage, AIMessage))
    ]
    recent_history = chat_history[-7:] 
    
    history_text = "\n".join([
        f"{'User' if isinstance(msg, HumanMessage) else 'AI'}: {msg.content}"
        for msg in recent_history[:-1]
        if isinstance(msg, (HumanMessage, AIMessage))
    ])
    
    if not history_text:
        history_text = "This is the start of the conversation."

    product_llm = llm.with_structured_output(Product, method="json_mode")
    prompt = f"""
        You are a product query parser for an e-commerce platform.
        Your ONLY job is to extract structured product search fields from the user's latest message.
        ═══════════════════════════════════════════
        OUTPUT SCHEMA
        ═══════════════════════════════════════════
        name  : The core product name (noun only, no adjectives, no brand unless user specifies)
        des: A JSON array of strings. Extract all descriptive attributes. If no description, output an empty array [].
        ═══════════════════════════════════════════
        CONTEXT INHERITANCE RULE
        ═══════════════════════════════════════════
        If the latest message is a follow-up or refinement (e.g., "what about blue?",
        "cheaper ones?", "in size M?"), resolve the BASE PRODUCT from conversation history
        and MERGE it with the new attributes from the latest message.

        Example:
        History:  User: "Show me waterproof jackets"
        Latest:   "Do you have them under $80 in black?"
        → name: "jacket", des: "waterproof, black"  ✓
        → name: "them", des: "black"                ✗

        ═══════════════════════════════════════════
        EXTRACTION EXAMPLES
        ═══════════════════════════════════════════
        "Do you have any red shirts"
        → name: "shirt",            des: ["red"],                   

        "waterproof jacket less than 50 usd"
        → name: "jacket",           des: ["waterproof"],           

        "How much is the Minimal Logo Tee?"
        → name: "Minimal Logo Tee", des: ["Minimal Logo"],          

        "Nike running shoes above $120"
        → name: "shoes",            des: ["Nike", "running"],        

        "something casual for summer under $40"
        → name: "clothing",         des: ["casual", "summer"],     

        [History: User asked for hoodies → AI listed hoodies]
        "how about in white, size L?"
        → name: "hoodie",           des: ["white", "size L"],        

        [History: User asked for hoodies → AI listed hoodies and ask "Would you like to see more details?"]
        "Yes, please"
        -> define name based on history, not latest message:

        ═══════════════════════════════════════════
        EDGE CASE RULES
        ═══════════════════════════════════════════
        - Brand names go into `des`, not `name`.           Nike shoes → name: "shoes", des: ["Nike"]
        - If name is truly unresolvable, use "unknown".
        - Never include adjectives in `name`.              "blue jacket" → name: "jacket", des: ["blue"]
        - Vague queries like "something cheap": name: "unknown", des: "cheap"
        - All of the product name is lowercased except for proper nouns or specific model names. "minimal logo tee" → name: "Minimal Logo Tee"

        ---------------History----------------
        {history_text}
        """
    for attempt in range(2):
        try:
            response = product_llm.invoke([
                SystemMessage(content=prompt),
                HumanMessage(content=state["user_prompt"]),
            ])
            break # Success! Break out of the loop
        except Exception as e:
            if attempt == 0:
                print(f"⚠️ Groq transient error caught: {e}. Retrying execution...")
                time.sleep(0.5) # Short backoff
                continue
            else:
                raise e # If it fails twice, raise it up
    
    action = ProductSearchAction().set_product(response)
    print(f"Product Context Extraction: {response}")
    
    return {"log_action": [action]}

def generate_product_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    product_model = action.get_product() if isinstance(action, ProductSearchAction) else None

    system_prompt = """
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
  - color         : TEXT
  - size          : TEXT

TABLE: product_skus (ps)
  - sku_id        : UUID, FK -> products sku 
  - quantity_available : INTEGER

JOINS (always use these):
  products p
  JOIN product_skus ps ON p.product_id = ps.product_id
  JOIN inventory i ON ps.sku_id = i.sku_id

═══════════════════════════════════════════
MANDATORY RULES (never violate)
═══════════════════════════════════════════
[R1] ALWAYS enforce operator precedence correctly. Group all search criteria inside a single parenthesis block combined with AND p.status = 1 to avoid leaking hidden/inactive products.
[R2] ALWAYS SELECT p.product_id, p.product_name, ps.price, p.description, p.status, ps.sku, ps.color, ps.size.
[R3] ALWAYS end with ORDER BY SIMILARITY(...) DESC LIMIT 10.
[R4] Output PLAIN SQL TEXT only — no markdown, no code fences, no comments.
[R5] If Description is empty, "N/A", or missing, OMIT the description ILIKE block entirely.
     Only generate ILIKE clauses for keywords that actually exist in the input.
     NEVER generate ILIKE '%%' — it matches everything and is forbidden.
═══════════════════════════════════════════
QUERY CONSTRUCTION GUIDE
═══════════════════════════════════════════

SELECT clause (always include):
  p.product_id, p.product_name, ps.price, p.description, p.status, ps.sku, ps.color, ps.size , i.quantity_available

WHERE clause — build with these blocks:

  ┌─ STATUS BLOCK (always include, placed first for performance) ───────┐
  │ p.status = 1                                                        │
  └─────────────────────────────────────────────────────────────────────┘

  ┌─ SEARCH CRITERIA BLOCK (always encapsulated with an AND) ───────────┐
  │ AND (                                                               │
  │   (p.product_name ILIKE '%<name>%' OR p.product_name ILIKE '%<des>%')│
  │   OR (                                                              │
  │     p.description ILIKE '%<keyword_1>%'                             │
  │     OR p.description ILIKE '%<keyword_2>%'                           │
  │     OR p.description ILIKE '%<semantic_synonym>%'                   │
  │   )                                                                 │
  │ )                                                                   │
  └─────────────────────────────────────────────────────────────────────┘



ORDER BY clause (always include):
  ORDER BY SIMILARITY(p.product_name, '<name> <des>') DESC LIMIT 10

═══════════════════════════════════════════
EXAMPLES
═══════════════════════════════════════════

INPUT:  product=jacket, des=waterproof
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status, ps.sku, ps.color, ps.size
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
WHERE p.status = 1
  AND (
    (p.product_name ILIKE '%jacket%')
    OR (
      p.description ILIKE '%waterproof%'
    )
  )
ORDER BY SIMILARITY(p.product_name, 'jacket') DESC LIMIT 10

---

INPUT:  product=shirt, des=red
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status, ps.sku, ps.color, ps.size
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
WHERE p.status = 1
  AND (
    (p.product_name ILIKE '%shirt%' OR p.product_name ILIKE '%red%')
    OR (
      p.description ILIKE '%shirt%'
      OR p.description ILIKE '%red%'
      OR p.description ILIKE '%crimson%'
      OR p.description ILIKE '%scarlet%'
    )
  )
ORDER BY SIMILARITY(p.product_name, 'shirt') DESC LIMIT 10

---

INPUT:  product=Minimal Logo Tee, des=Minimal Logo
OUTPUT:
SELECT p.product_id, p.product_name, ps.price, p.description, p.status, ps.sku, ps.color, ps.size
FROM products p
JOIN product_skus ps ON p.product_id = ps.product_id
WHERE p.status = 1
  AND (
    (p.product_name ILIKE '%Minimal Logo Tee%' OR p.product_name ILIKE '%Minimal Logo%')
    OR (
      p.description ILIKE '%minimal%'
      OR p.description ILIKE '%logo%'
      OR p.description ILIKE '%tee%'
      OR p.description ILIKE '%graphic tee%'
    )
  )
ORDER BY SIMILARITY(p.product_name, 'Minimal Logo Tee') DESC LIMIT 10
"""

    convert_prompt = f"""
Generate a SQL query for the following product search:

Product name : {product_model.name}
Description  : {product_model.des}
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
    print(f"Raw DB Results: {results}")
    # If DB has an error or found nothing
    if not results or "error" in results[0] or "Executed (no data)" in str(results[0].get("status", "")):
        fallback_msg = "I couldn't find any products matching your request at the moment. Please try different keywords!"
        return {
            "answer": fallback_msg,                    # Trả đúng kiểu string
            "ui_data": {"product_ids": []},            # Dữ liệu UI trống
            "messages": [AIMessage(content=fallback_msg)]
        }

    # If data exists, use Structured Output to create the MCP JSON
    ui_llm = llm.with_structured_output(UIResponse, method="json_mode")
    
    system_prompt = system_prompt = """You are a helpful e-commerce assistant of Sanquo.
You receive data from the database containing products matching the user's request. The currency is VND

OUTPUT SCHEMA as JSON FORM:
    - answer: Your given answer based on the user query and context retrieved
    - product_ids: A list of all product_id values extracted from the database results

YOUR TASK:
1. Give answer base on the data, but keep it concise and natural. Don't just read out the product names — synthesize a helpful response.
2. The context data may contain more than you need or more than user need. 
    Focus on relevance and helpfulness, not exhaustiveness. 
    If the results are not relevant, it's better to say "I couldn't find any products matching your request" than to list irrelevant items.
    For example if the user asked for "red shirts" but the results are mostly blue pants, it's better to say you found nothing than to list irrelevant products.
3. Extract all the `product_id` values from the database results and return them as a list of integers in the `product_ids` field.
4. The 'answer' field should be a string in the format:
"I found products matching your request. 
- The 'Product A' is $Y, available in [color/size]. Some great information about the product is: [short-parahphased description].
- The 'Product B' is $Z, available in [color/size]. Some great information about the product is: [short-parahphased description].
Would you like to see more details about any of these products?"

    """
    
    response_model = ui_llm.invoke([
        SystemMessage(content=system_prompt + f"Data Results: {results}"),
        HumanMessage(content=state["user_prompt"])
    ])
    non_duplicate_product_ids = set(response_model.product_ids)
    print("passed")
    # 3. Trả về rành mạch từng field vào MasterState
    return {
        "answer": response_model.answer,
        "ui_data": {"product_ids": list(non_duplicate_product_ids)},
        "messages": [AIMessage(content=response_model.answer)]
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