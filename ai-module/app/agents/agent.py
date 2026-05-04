# app/agents/agent.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from langgraph.checkpoint.memory import MemorySaver
from langchain_core.runnables import RunnableConfig

# Import Core
from app.agents.state import MasterState, IntentOutput
from app.agents.config import llm

# Import Subgraphs đã compile
from app.agents.subgraphs.general import general_graph_app
from app.agents.subgraphs.product import product_graph_app
from app.agents.subgraphs.order import order_graph_app
from app.agents.subgraphs.policy import policy_graph_app

# --- 1. NODE PHÂN LOẠI INTENT ---
def intent_classifier(state: MasterState,config: RunnableConfig) -> MasterState:
    user_id = config["configurable"].get("thread_id")

    chat_history = [
        msg for msg in state["messages"] 
        if isinstance(msg, (HumanMessage, AIMessage))
    ]
    # Lấy 5 tin nhắn gần nhất để tránh tràn token và giảm nhiễu
    recent_history = chat_history[-5:] 
    
    # Format lịch sử thành một chuỗi text dễ đọc cho LLM
    history_text = "\n".join([
        f"{'User' if isinstance(msg, HumanMessage) else 'AI'}: {msg.content}" 
        for msg in recent_history[:-1] # Trừ câu cuối cùng (câu hiện tại) ra
    ])
    
    if not history_text:
        history_text = "This is the start of conversation"

    prompt = """
You are an intent classification engine for a production e-commerce AI assistant.
Your ONLY job is to output ONE intent label. No explanation. No extra text.

═══════════════════════════════════════════
CONVERSATION HISTORY (most recent last):
{history_text}
═══════════════════════════════════════════

═══════════════════════════════════════════
INTENT DEFINITIONS
═══════════════════════════════════════════

[product_search]
Trigger when the user wants to FIND, BROWSE, FILTER, or COMPARE products.
Covers:
  • Direct product queries          → "Show me Nike shoes under $100"
  • Category browsing               → "What laptops do you have?"
  • Attribute filtering             → "In blue?", "Cheaper ones?", "Size M?"
  • Availability checks             → "Do you stock waterproof jackets?"
  • Product comparisons             → "Which is better, A or B?"
  • Recommendations                 → "What's popular?", "Best gaming mouse?"
  • Follow-ups refining a search    → "What about red?", "Under $50?"

[order_tracking]
Trigger when the user references a SPECIFIC ORDER they have placed.
Covers:
  • Status / location queries       → "Where is my order?", "Has it shipped?"
  • Tracking requests               → "Track order #12345"
  • Cancellation requests           → "Cancel my order"
  • Cancellation confirmation       → "Did my cancellation go through?"
  • Refund status                   → "When will I get my refund?"
  • Delivery issues                 → "My package hasn't arrived"
  • Modifying an existing order     → "Change the address on my order"

[policy_question]
Trigger when the user asks about HOW THE STORE OPERATES — rules, procedures, or support.
Covers:
  • Return / exchange policy        → "Can I return this after 30 days?"
  • Shipping & delivery times       → "How long does standard shipping take?"
  • Payment methods                 → "Do you accept Visa / PayPal / COD?"
  • Refund procedures               → "How do I request a refund?"
  • Privacy & data                  → "Is my information safe?"
  • Warranty / guarantees           → "Is there a warranty?"
  • Technical / account support     → "How do I reset my password?"
  • Promotions & vouchers           → "How do I apply a discount code?"

[general]
Trigger for EVERYTHING that does not fit the above.
Covers:
  • Greetings                       → "Hi", "Hello", "Good morning"
  • Farewells / thanks              → "Bye", "Thanks!", "You're helpful"
  • Bot identity questions          → "Who are you?", "Are you a robot?"
  • Unrelated small talk            → "What's the weather?", "Tell me a joke"
  • Ambiguous / empty messages      → "...", "ok", "hmm"

═══════════════════════════════════════════
CLASSIFICATION RULES (apply in order)
═══════════════════════════════════════════

RULE 1 — CONTEXT INHERITANCE (most important)
  If the latest message is short or ambiguous (e.g., "what about blue?",
  "cheaper?", "how much?", "yes", "that one"), resolve its intent using the
  CONVERSATION HISTORY — do NOT default to [general].

  Example:
    History:  User asked about jackets → AI listed jacket options
    Message:  "do you have them in black?"
    Intent:   product_search  ✓   (NOT general ✗)

RULE 2 — ORDER vs POLICY DISAMBIGUATION
  • References a specific, already-placed order  → order_tracking
  • Asks about how the store handles orders/refunds in general → policy_question

  Example:
    "What is your refund policy?"         → policy_question
    "I want a refund for order #999"      → order_tracking

RULE 3 — COMPOUND MESSAGES
  If a message spans two intents, pick the PRIMARY action the user wants.

  Example:
    "What's your return policy and do you have blue hoodies?"
    Primary action: product_search (the product question is more actionable)

RULE 4 — UNCERTAINTY FALLBACK
  If you genuinely cannot determine the intent even after applying Rules 1–3,
  output: general

═══════════════════════════════════════════
OUTPUT FORMAT
═══════════════════════════════════════════
Return exactly one of:
  product_search | order_tracking | policy_question | general
"""
    message = state['user_prompt']
    intent_llm = llm.with_structured_output(IntentOutput)

    response = intent_llm.invoke([
        SystemMessage(content = prompt),
        HumanMessage(content = message)
    ])

    return {
        "intent":response.intent,
        "customer_id": user_id
    }

# --- 2. XÂY DỰNG MASTER GRAPH ---
workflow = StateGraph(MasterState)

# Master Nodes
workflow.add_node("intent_classifier", intent_classifier)
workflow.add_node("general_agent", general_graph_app)
workflow.add_node("product_agent", product_graph_app)
workflow.add_node("order_agent", order_graph_app)
workflow.add_node("policy_agent", policy_graph_app)

workflow.add_edge(START, "intent_classifier")

# Conditional Router
workflow.add_conditional_edges(
    "intent_classifier",
    lambda state: state["intent"],
    {
        "product_search": "product_agent",
        "order_tracking": "order_agent",
        "policy_question": "policy_agent",
        "general": "general_agent"
    }
)

# Thoát ra ngoài
workflow.add_edge("product_agent", END)
workflow.add_edge("order_agent", END)
workflow.add_edge("policy_agent", END)
workflow.add_edge("general_agent", END)

# Biên dịch Master Graph
checkpointer = MemorySaver()
app = workflow.compile(checkpointer=checkpointer)

if __name__ == "__main__":
    print("===  AGENT STARTED ===")
    print("Type 'exit' to quit.\n")

    # 1. Define a thread ID. In a real app, this would be the User ID or Session ID.
    user_id = 1
    config = {"configurable": {"thread_id": user_id}}

    while True:
        try:
            user_input = input("User: ")
            if user_input.lower() in ["exit", "quit"]:
                break
            
            # 2. Prepare the input
            # We ONLY send the new message. LangGraph automatically pulls 
            # the *old* messages from memory because we provided the thread_id.
            input_payload = {"user_prompt": user_input,
                             "log_action":[],
                             "messages": [HumanMessage(content=user_input)]
                            }
            
            # 3. Invoke with config
            # usage of 'config' is CRITICAL for memory
            result = app.invoke(input_payload, config=config)
            
            # Extract the final response
            final_msg = result["answer"]
            print(f"Assistant: {final_msg}\n")
            
        except Exception as e:
            print(f"An error occurred: {e}")
            # Optional: Print stack trace for debugging
            import traceback
            traceback.print_exc()