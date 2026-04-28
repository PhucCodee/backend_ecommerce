# app/agents/subgraphs/order.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage
from psycopg2.extras import RealDictCursor

# Import từ core
from app.agents.state import MasterState, OrderTrackingAction, Order
from app.agents.config import llm, get_db_connection

def order_tracking(state: MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Order search Agent ---")
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

    order_llm = llm.with_structured_output(Order)
    prompt = f"""
    You are an AI assistant for an e-commerce platform. Extract order tracking properties based on the user query.

    ---------------------------------------
    CONVERSATION CONTEXT:
    {history_text}
    ---------------------------------------

    Examples:
    User: "Is my newest order shipping?" -> order_number: "", time_context: "newest", status: "shipping"
    User: "Track my order #ORD-9876" -> order_number: "ORD-9876", time_context: "any", status: "any"
    User: "I just canceled my order yesterday, was it successful?" -> order_number: "", time_context: "yesterday", status: "cancel"
    """
    
    response = order_llm.invoke([
        SystemMessage(content=prompt),
        HumanMessage(content=state['user_prompt']),
    ])
    
    action = OrderTrackingAction(order=response)
    print(f"Order Entity Extracted: {response}")
    return {"log_action": [action]}

def generate_order_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    order_model = action.get_order() if isinstance(action, OrderTrackingAction) else Order()

    system_prompt = f"""You are a smart PostgreSQL expert.
    Add the appropriate WHERE, ORDER BY, and LIMIT clauses based on the user's request.

    BASE SQL:
    SELECT o.order_id, o.order_number, o.status AS order_status, o.total_amount, o.created_at,
           oi.product_name, oi.quantity, oi.unit_price,
           os.address_line1, os.city,
           of.tracking_number, of.carrier, of.shipped_at
    FROM orders o
    JOIN order_items oi ON o.order_id = oi.order_id
    LEFT JOIN order_shipping os ON o.order_id = os.order_id
    LEFT JOIN order_fulfillment of ON o.order_id = of.order_id

    CRITICAL RULES:
    1. You MUST ALWAYS include: WHERE o.user_id = {state['customer_id']}
    2. If order_number is provided ({order_model.order_number}), filter by o.order_number.
    3. Order status is stored as INTEGER: 0=created, 1=confirmed, 2=processing, 3=shipped, 4=delivered, 5=cancelled, 6=failed. Map accordingly.
    4. If time_context is 'newest', add ORDER BY o.created_at DESC LIMIT 1.
    5. Return ONLY the plain SQL text, no markdown formatting.
    """
    
    result = llm.invoke([
        SystemMessage(content=system_prompt),
        HumanMessage(content=f"User's raw prompt: {state['user_prompt']}")
    ])

    if isinstance(action, OrderTrackingAction):
        action.set_query(result.content.strip())
        
    print(f"Order SQL Query: {result.content}")
    return {}

def execute_order_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    query = action.get_query() if isinstance(action, OrderTrackingAction) else ""
    
    conn = get_db_connection()
    if not conn:
        if isinstance(action, OrderTrackingAction): action.set_order_res([{"error": "DB Connection Failed"}])
        return {}
    
    try:
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            if not query.lower().startswith(("select", "with")): return {}
            cur.execute(query)
            clean_results = [dict(row) for row in cur.fetchall()] if cur.description else [{"status": "Executed (no data)"}]
            
            if isinstance(action, OrderTrackingAction):    
                action.set_order_res(clean_results)
            conn.commit()
            
    except Exception as e:
        print(f"Lỗi SQL: {e}")
        if isinstance(action, OrderTrackingAction): action.set_order_res([{"error": str(e)}])
    finally:
        if conn: conn.close()
        
    return {}
    
def synthesize_order_answer(state: MasterState):
    action = state["log_action"][-1]
    results = action.get_order_res()

    # Extract order IDs and order numbers for ui_data
    order_ids = []
    order_numbers = []
    
    if results and isinstance(results, list):
        for result in results:
            if isinstance(result, dict):
                if "order_id" in result and result["order_id"]:
                    order_ids.append(result["order_id"])
                if "order_number" in result and result["order_number"]:
                    order_numbers.append(result["order_number"])

    system_prompt = """You are a professional and empathetic customer service agent for an e-commerce platform.
    Your task is to read the database results of the customer's order and explain it to them naturally.

    GUIDELINES:
    1. If the results are empty `[]` or contain an error, politely inform the customer that you cannot find the order. Ask them to verify the order number.
    2. Do NOT show technical IDs, SQL queries, or database jargon.
    3. Format the response clearly using bullet points for items if necessary.
    4. Highlight important information like Order Status, Tracking Number, and Total Amount.
    
    Format Example:
    "Here is the information for your order **#ORD-123**:
    - **Status:** Shipped
    - **Total:** $150.00
    - **Tracking Number:** [Carrier] 1Z9999999999
    
    Items in this order:
    - 1x Blue Jacket ($150.00)"

    CRITICAL DB SCHEMA INFO - ENUMS:
    - Order status is stored as INTEGER: 0=created, 1=confirmed, 2=processing, 3=shipped, 4=delivered, 5=cancelled, 6=failed.
    - The DB returns status as an integer (0=created, 3=shipped, etc.). You MUST translate this integer into a natural language word when replying to the user.
    """
    
    response = llm.invoke([
        SystemMessage(content=system_prompt + f"\nData Results: {results}"),
        HumanMessage(content=state["user_prompt"])
    ])
    
    # 🔄 Return consistent format with ui_data
    return {
        "answer": response.content,
        "ui_data": {
            "order_ids": order_ids,
            "order_numbers": order_numbers
        },
        "messages": [AIMessage(content=response.content)]
    }

# ==========================================
# BUILD SUBGRAPH
# ==========================================
order_workflow = StateGraph(MasterState)
order_workflow.add_node("order_tracking", order_tracking)
order_workflow.add_node("generate_order_query", generate_order_query)
order_workflow.add_node("execute_order_query", execute_order_query)
order_workflow.add_node("synthesize_order_answer", synthesize_order_answer)

order_workflow.add_edge(START, "order_tracking")
order_workflow.add_edge("order_tracking", "generate_order_query")
order_workflow.add_edge("generate_order_query", "execute_order_query")
order_workflow.add_edge("execute_order_query", "synthesize_order_answer")
order_workflow.add_edge("synthesize_order_answer", END)

order_graph_app = order_workflow.compile()