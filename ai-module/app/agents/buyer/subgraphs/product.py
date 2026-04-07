# app/agents/buyer/subgraphs/product.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage
from psycopg2.extras import RealDictCursor

# Import from core
from app.agents.buyer.state import MasterState, ProductSearchAction, Product
from app.agents.buyer.config import llm, get_db_connection

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
    You are an AI assistant of an e-commerce platform. You are handling a product search request.
    
    ---------------------------------------
    CONVERSATION CONTEXT (Recent History):
    {history_text}
    ---------------------------------------

    CRITICAL INSTRUCTIONS:
    - If the user's latest query is a continuation or modification (e.g., "how about blue?", "cheaper ones"), you MUST look at the CONTEXT to determine the base product name and merge it with the new requirements.

    Example 1: User: "Do you have any red shirts" -> name: shirt, price: ("unknown",0), des: red
    Example 2: User: "waterproof jacket less than 50 usd" -> name: jacket, price: ("less",50), des: waterproof
    Example 3: User: "How much is the Minimal Logo Tee" -> name: Minimal Logo Tee, price: ("ask",0), des: Minimal Logo
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

    unknown_price = product_model.price[0] == "unknown"
    ask_price = product_model.price[0] == "ask"

    convert_prompt = f"Write a sql query to search for product: {product_model.name}"
    price_prompt = "" if unknown_price else (f"I want to know its price" if ask_price else f"\nwhich has price {product_model.price[0]} {product_model.price[1]} dollar")
    convert_prompt += price_prompt
    convert_prompt += f"\nThe description column must have word {product_model.des} or something semantically similar"

    system_prompt = f"""You are a smart PostgreSQL expert.
    Your goal is to generate a correct and sufficient SQL query based on the user's question.

    CRITICAL DB SCHEMA INFO - ENUMS:
    - Product status is stored as INTEGER: 0=draft, 1=active, 2=inactive, 3=removed, 4=archived.
    - RULE: You MUST ALWAYS add a condition to filter `p.status = 1` (Active products only).

    CRITICAL SELECTION RULE:
    - You MUST ALWAYS select `p.product_id` in your query. This is mandatory for the UI frontend.

    Example:
    Human: "Write a sql query to search for product: jacket
            Which has price equal 100 dollars
            The description must have word waterproof or something semantically similar"
    --> 
    AI: SELECT p.product_id, p.product_name, ps.price, p.description, p.status
    FROM products p JOIN product_skus ps 
        ON p.product_id = ps.product_id JOIN inventory i 
        ON ps.sku_id = i.sku_id 
    WHERE (p.product_name ILIKE '%waterproof%' OR p.product_name ILIKE '%jacket%') 
        AND ps.price = 100 AND p.status = 1
        OR (p.description ILIKE '%waterproof%' 
        OR p.description ILIKE '%jacket%' 
        OR p.description ILIKE '%rain%' 
        OR p.description ILIKE '%windbreaker%') 
    ORDER BY SIMILARITY(p.product_name, 'waterproof jacket') 
    DESC LIMIT 5 

    CRITICAL NOTE: 
    - For p.description condition always use OR
    - If user wants to retrieve the product's price, skip the ps.price condition
    - Return ONLY the full SQL query which can be executed directly (PLAIN SQL TEXT, no markdown)
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
    
    system_prompt = """You are a helpful e-commerce assistant.
    You receive data from the database containing products matching the user's request.
    
    YOUR TASK:
    1. Write a short, friendly message in the `text` field (e.g., "I found some great options that match your expectations!"). Do not use exactly the eg sentence, you should paraphase in a polite manner
    2. Extract the product information from the data and map it to the `ui_components` list.
    3. Ensure EACH component has type="product_embed" and the `data` field contains `product_id`, `name`, `price`, and a short `description`.
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
    
    # 1. Convert Pydantic model to JSON string to push to Frontend via FastAPI
    json_output = response_model.model_dump()
    
    # 2. Save the natural text to history to maintain AI memory
    return {
        "answer": json_output,
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