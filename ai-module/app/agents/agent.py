# app/agents/agent.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage, ToolMessage
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
    recent_history = chat_history[-7:] 
    
    # Format lịch sử thành một chuỗi text dễ đọc cho LLM
    history_text = "\n".join([
        f"{'User' if isinstance(msg, HumanMessage) else 'AI'}: {msg.content}"
        for msg in recent_history[:-1]
        if isinstance(msg, (HumanMessage, AIMessage)) # Bỏ qua ToolMessage
    ])
    
    if not history_text:
        history_text = "This is the start of conversation"
    history_text = history_text.replace("AI: \n", "")
    prompt = """
You are an intent classification engine for a production fashion e-commerce AI assistant of platform named Sanquo .
Your ONLY job is to output ONE intent label

INTENT DEFINITIONS
═══════════════════════════════════════════
[policy_question]
Trigger when the user asks about HOW THE STORE OPERATES — rules, procedures, or support.
Covers:
  • Return / exchange policy        → "Can I return clothes that don't fit?"
  • Sizing & fit guidance           → "How do your sizes run?", "Do you have a size guide?"
  • Shipping & delivery times       → "How long does delivery take?"
  • Payment methods                 → "Do you accept Visa / MoMo / COD?"
  • Refund procedures               → "How do I request a refund?" "How can i return my order?"
  • Privacy & data                  → "Is my information safe?"
  • Warranty / quality guarantee    → "What if the stitching comes apart?"
  • Technical / account support     → "How do I reset my password?"
  • Promotions & vouchers           → "How do I apply a discount code?"
  • Guidance on using the platform  → "How do I create an account?", "How to contact customer service?"
  • Technical issue queries         → "How can i get help with technical issue"
  • Introduction                    → "What is Sanquo?"
  
[product_search]
Trigger when the user wants to FIND, BROWSE, FILTER, or COMPARE clothing items.
Covers:
  • Direct product queries          → "Show me women's dresses under 500k"
  • Category browsing               → "What types of jeans do you have?"
  • Attribute filtering             → "In white?", "Cheaper ones?", "Size M?", "Slim fit?"
  • Availability checks             → "Do you stock oversized hoodies?"
  • Product comparisons             → "Which is better, the linen or cotton shirt?"
  • Recommendations                 → "What's trending?", "Best outfit for the office?"
  • Follow-ups refining a search    → "What about in black?", "Under 300k?"
  • Outfit / styling queries        → "What matches with these pants?"
  

[order_tracking]
Trigger when the user references a SPECIFIC ORDER they have placed.
Covers:
  • Status / location queries       → "Where is my order?", "Has it shipped?"
  • Tracking requests               → "Track order #12345"
  • Cancellation requests           → "Cancel my order"
  • Cancellation confirmation       → "Did my cancellation go through?"
  • Refund status                   → "When will I get my refund?"
  • Delivery issues                 → "My package hasn't arrived"
  • Modifying an existing order     → "Change the size on my order", "Update delivery address"




═══════════════════════════════════════════
CLASSIFICATION RULES (apply in order)
═══════════════════════════════════════════

STEP TO FOLLOW — 
1. DEFINE THE INTENT BASED ON THE LATEST USER MESSAGE AND THE CONVERSATION HISTORY:
  If the latest message is short or ambiguous (e.g., "what about blue?",
  "cheaper?", "size M?", "yes", "that one"), resolve its intent using the
  CONVERSATION HISTORY 

  Example:
    History:  User asked about summer dresses → AI listed dress options
    Message:  "do you have them in white?"
    Intent:   product_search  ✓   

  Example:
    History:  User asked about summer jeans → AI listed dress options and say "Do you want to see more? Would you like to see more details about any of these products?"
    Message:  "Yes"
    Intent:   product_search  ✓  
2. IF THE LATEST USER MESSAGE REFERENCES AN ORDER NUMBER OR SPECIFIC ORDER DETAILS, CLASSIFY AS [order_tracking]
3. IF THE LATEST USER MESSAGE ASKS TO BROWSE, FILTER, COMPARE, OR GET RECOMMENDATIONS ABOUT PRODUCTS, CLASSIFY AS [product_search]
4. IF THE LATEST USER MESSAGE ASKS ABOUT STORE POLICIES, PROCEDURES, OR SUPPORT, CONTACT, CLASSIFY AS [policy_question]


---------------History----------------
{history_text}
═══════════════════════════════════════════
OUTPUT FORMAT
═══════════════════════════════════════════
Return exactly one of:
  product_search | order_tracking | policy_question
"""

    print(history_text)
    intent_llm = llm.with_structured_output(IntentOutput)

    response = intent_llm.invoke([
        SystemMessage(content = prompt),
        HumanMessage(content = state["user_prompt"])
    ])
    print(f"Intent Classifier Output: {response.intent}")
    return {
        "intent":response.intent,
        "customer_id": user_id
    }

# --- 2. XÂY DỰNG MASTER GRAPH ---
workflow = StateGraph(MasterState)

# Master Nodes
workflow.add_node("intent_classifier", intent_classifier)
# workflow.add_node("general_agent", general_graph_app)
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
    }
)

# Thoát ra ngoài
workflow.add_edge("product_agent", END)
workflow.add_edge("order_agent", END)
workflow.add_edge("policy_agent", END)
# workflow.add_edge("general_agent", END)

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
            print(f"Data: {result.get('ui_data', {})}\n")

        except Exception as e:
            print(f"An error occurred: {e}")
            # Optional: Print stack trace for debugging
            import traceback
            traceback.print_exc()