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

    prompt = f"""You are an order query parser for an e-commerce platform.
Extract structured tracking fields from the user's latest message.

CONVERSATION HISTORY (recent, oldest→newest):
{history_text}

---
FIELD DEFINITIONS

order_number   : Exact order ID if explicitly stated (e.g. "ORD-2025-001234"). Use "" if absent.

time_context   : Temporal scope the user implies.
                 Options: any | newest | oldest | today | yesterday | this_week | last_week | this_month
                 Default: "any"

order_intent   : User's primary goal.
                 Options:
                   track_status   → wants to know current order status
                   track_location → asks about package location, carrier, or tracking number
                   cancel_order   → wants to cancel or initiate cancellation
                   request_refund → requests or asks about a refund
                   confirm_action → verifies whether a past action succeeded (e.g. "did my cancellation go through?")


---
CONTEXT INHERITANCE
If the latest message is a follow-up (e.g. "what about the other one?", "did it arrive?"),
resolve the referenced order from the conversation history above.

---
EXAMPLES

"Where is my order?"
→ order_number: "",              time_context: "any",       
  order_intent: "track_location"

"Is my newest order still processing?"
→ order_number: "",              time_context: "newest",    
  order_intent: "track_status"

"Track ORD-2025-009876"
→ order_number: "ORD-2025-009876", time_context: "any",     
  order_intent: "track_location"

"I cancelled my order yesterday — did it go through?"
→ order_number: "",              time_context: "yesterday", 
  order_intent: "confirm_action"

"Cancel my order from this week"
→ order_number: "",              time_context: "this_week",  
  order_intent: "cancel_order"

"Has my package from last week been delivered?"
→ order_number: "",              time_context: "last_week",  
  order_intent: "track_status"      

[History: User asked about ORD-2025-005 → AI confirmed it was shipped]
"What's the tracking number?"
→ order_number: "ORD-2025-005", time_context: "any",        
  order_intent: "track_location"     
"""
    order_llm = llm.with_structured_output(Order)

    response = order_llm.invoke([
        SystemMessage(content=prompt),
        HumanMessage(content=state["user_prompt"]),
    ])

    print(f"Order response: {response}")

    action = OrderTrackingAction(human_mes=state["user_prompt"], order=response)
    print(f"Order Entity Extracted: {response}")

    return {"log_action": [action]}

def generate_order_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    order_model = action.get_order() if isinstance(action, OrderTrackingAction) else Order()

    system_prompt = f"""You are a PostgreSQL expert. Generate a valid SQL query to fetch the customer's order data.

BASE QUERY (do not modify the SELECT or JOINs):
SELECT
    o.order_id, o.order_number, o.status AS order_status,
    o.total_amount, o.created_at,
    oi.product_name, oi.quantity, oi.unit_price,
    os.address_line1, os.city,
    of.tracking_number, of.carrier, of.shipped_at
FROM orders o
JOIN order_items oi            ON o.order_id = oi.order_id
LEFT JOIN order_shipping os    ON o.order_id = os.order_id
LEFT JOIN order_fulfillment of ON o.order_id = of.order_id

RULES:
1. ALWAYS include:             WHERE o.user_id = {state['customer_id']}
2. order_number filter:        ONLY add AND o.order_number = '...' if order_number is NOT an empty string "".
                               If order_number = "", skip this clause entirely.                          
3. time_context mapping:
   - newest     → ORDER BY o.created_at DESC LIMIT 1
   - oldest     → ORDER BY o.created_at ASC  LIMIT 1
   - today      → AND o.created_at::date = CURRENT_DATE
   - yesterday  → AND o.created_at::date = CURRENT_DATE - INTERVAL '1 day'
   - this_week  → AND o.created_at >= date_trunc('week', CURRENT_DATE)
   - last_week  → AND o.created_at >= date_trunc('week', CURRENT_DATE) - INTERVAL '1 week'
                  AND o.created_at <  date_trunc('week', CURRENT_DATE)
   - this_month → AND o.created_at >= date_trunc('month', CURRENT_DATE)
   - any        → no time filter
4. Output ONLY raw SQL — no markdown, no explanation, no backticks.
"""
    
    # Thay thế HumanMessage cũ
    user_message = f"""Generate SQL for this order request:
    - order_number  : "{order_model.order_number}"
    - time_context  : "{order_model.time_context}""
    - order_intent  : "{order_model.order_intent}""
    """

    result = llm.invoke([
        SystemMessage(content=system_prompt),
        HumanMessage(content=user_message)
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
    
import json
from decimal import Decimal
from datetime import datetime, date

def serialize_results(results: list) -> str:
    """Convert Decimal/datetime to JSON-serializable types."""
    def default(obj):
        if isinstance(obj, Decimal):
            return float(obj)
        if isinstance(obj, (datetime, date)):
            return obj.isoformat()
        return str(obj)
    return json.dumps(results, default=default, ensure_ascii=False, indent=2)


def synthesize_order_answer(state: MasterState):
    action = state["log_action"][-1]
    results = action.get_order_res()
    print(results)
    order_model = action.get_order() if isinstance(action, OrderTrackingAction) else Order()

    order_ids     = list({r["order_id"]     for r in results if isinstance(r, dict) and r.get("order_id")})
    order_numbers = list({r["order_number"] for r in results if isinstance(r, dict) and r.get("order_number")})

    def to_serializable(obj):
        if isinstance(obj, Decimal):  return float(obj)
        if isinstance(obj, (datetime, date)): return obj.isoformat()
        return str(obj)

    serialized = json.dumps(results, default=to_serializable, ensure_ascii=False, indent=2)

    # ── Intent-aware answering focus ──────────────────────────────────────────
    INTENT_GUIDANCE = {
        "track_status":   "Focus on the current order status. If it hasn't shipped yet, say so clearly.",
        "track_location": "Focus on tracking number, carrier, and shipping address. If not shipped yet, explain the order hasn't left the warehouse.",
        "cancel_order":   "Focus on whether the order is still cancellable (only if status is Created or Confirmed).",
        "request_refund": "Focus on refund eligibility and payment status.",
        "confirm_action": "Directly confirm whether the action (cancellation, refund, etc.) was successfully applied based on the current status.",
    }
    intent_note = INTENT_GUIDANCE.get(order_model.order_intent, "")

    system_prompt = f"""You are a friendly, professional customer support agent for an e-commerce platform.
The ORDER DATA below is real and retrieved from the database. Present it clearly and answer the customer's actual question.

CUSTOMER INTENT: {order_model.order_intent}
ANSWERING FOCUS: {intent_note}

ENUM TRANSLATIONS — always apply:
- order_status : 0=Created, 1=Confirmed, 2=Processing, 3=Shipped, 4=Delivered, 5=Cancelled, 6=Failed
- currency     : 0=₫ (VND), 1=$ (USD), 2=€ (EUR)

RULES:
1. Never expose raw IDs, integers, column names, or SQL terms.
2. Skip null fields silently.
3. ONLY apologize if ORDER DATA is literally [] or contains an "error" key.
4. Bold key details: order number, status, total.
5. Always directly answer the customer's question first, then show order details.

OUTPUT FORMAT:
<direct answer to the customer's question>

Order **#ORD-XXXXXX** details:
- **Status:** Created
- **Total:** 1,936,000₫
- **Placed on:** 2026-05-04

Items:
- 4× Pleated Midi Skirt — 440,000₫ each

Shipping to: 123 Nguyen Trai, Ho Chi Minh City
"""

    response = llm.invoke([
        SystemMessage(content=system_prompt),
        HumanMessage(content=f"Customer question: {state['user_prompt']}\n\nORDER DATA:\n{serialized}")
    ])

    return {
        "answer": response.content,
        "ui_data": {"order_ids": order_ids, "order_numbers": order_numbers},
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