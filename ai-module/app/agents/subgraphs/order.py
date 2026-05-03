# app/agents/subgraphs/order.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage
from psycopg2.extras import RealDictCursor

# Import từ core
from app.agents.state import MasterState, OrderTrackingAction, Order
from app.agents.config import llm, get_db_connection

# ── Node ─────────────────────────────────────────────────────────────────────

def order_tracking(state: MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Order Tracking Agent ---")

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


    # ── Prompt ───────────────────────────────────────────────────────────────────

    prompt = f"""
You are an order query parser for an e-commerce platform.
Extract structured order tracking fields from the user's latest message.

═══════════════════════════════════════════
CONVERSATION HISTORY (most recent last):
{history_text}
═══════════════════════════════════════════

═══════════════════════════════════════════
OUTPUT SCHEMA
═══════════════════════════════════════════
order_number   : Exact order ID if user mentions one (e.g. "ORD-2025-001234").
                 Use "" if not mentioned.

time_context   : Which order in time the user is referring to.
                 Values: any | newest | oldest | today | yesterday | this_week | last_week | this_month
                 Default: "any" (unless user implies a time reference)

status_filter  : The order status the user is asking about or filtering by.
                 Values: any | pending | confirmed | processing | shipped | delivered | cancelled | refunded
                 Default: "any"

order_intent   : The user's primary goal.
                 Values:
                   track_status   → asking for current order status
                   track_location → asking where package is / tracking number
                   cancel_order   → requesting cancellation or confirming if it was cancelled
                   request_refund → requesting refund or asking refund status
                   confirm_action → confirming a recent action (e.g. "did my cancellation go through?")

needs_tracking : true if user asks about package location, carrier, tracking number, or delivery ETA.
needs_refund   : true if user asks about or requests a refund.

═══════════════════════════════════════════
CONTEXT INHERITANCE RULE
═══════════════════════════════════════════
If the latest message is a follow-up (e.g., "what about the other one?", "did it ship?"),
resolve the referenced order from CONVERSATION HISTORY.

═══════════════════════════════════════════
EXAMPLES
═══════════════════════════════════════════
"Where is my order?"
→ order_number: "",           time_context: "any",       status_filter: "any",
  order_intent: "track_location", needs_tracking: true,  needs_refund: false

"Is my newest order still being processed?"
→ order_number: "",           time_context: "newest",    status_filter: "processing",
  order_intent: "track_status",   needs_tracking: false, needs_refund: false

"Track order ORD-2025-009876"
→ order_number: "ORD-2025-009876", time_context: "any", status_filter: "any",
  order_intent: "track_location",  needs_tracking: true, needs_refund: false

"I cancelled my order yesterday, was it successful?"
→ order_number: "",           time_context: "yesterday", status_filter: "cancelled",
  order_intent: "confirm_action",  needs_tracking: false, needs_refund: false

"I want to cancel my order this week"
→ order_number: "",           time_context: "this_week", status_filter: "any",
  order_intent: "cancel_order",    needs_tracking: false, needs_refund: false

"When will I get my refund for ORD-2025-001100?"
→ order_number: "ORD-2025-001100", time_context: "any", status_filter: "refunded",
  order_intent: "request_refund",  needs_tracking: false, needs_refund: true

"Has my package from last week been delivered?"
→ order_number: "",           time_context: "last_week", status_filter: "delivered",
  order_intent: "track_status",    needs_tracking: false, needs_refund: false

[History: User asked about order ORD-2025-005 → AI said it was shipped]
"What's the tracking number?"
→ order_number: "ORD-2025-005", time_context: "any",   status_filter: "shipped",
  order_intent: "track_location",  needs_tracking: true, needs_refund: false
"""
    order_llm = llm.with_structured_output(Order)

    response = order_llm.invoke([
        SystemMessage(content=prompt),
        HumanMessage(content=state["user_prompt"]),
    ])

    print(f"Order response: {response}")

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

    print("Execute sql done")    
    return {}
    
def synthesize_order_answer(state: MasterState):
    action = state["log_action"][-1]
    results = action.get_order_res()
    print("Checkpount3")
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

    print("Checkpount4")
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
    print("results",results)
    response = llm.invoke([
        SystemMessage(content=system_prompt + f"\nData Results: {results}"),
        HumanMessage(content=state["user_prompt"])
    ])
    print("Last checkpoint")
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