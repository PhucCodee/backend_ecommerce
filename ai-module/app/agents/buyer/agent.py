# app/agents/buyer/agent.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from langgraph.checkpoint.memory import MemorySaver
from langchain_core.runnables import RunnableConfig

# Import Core
from app.agents.buyer.state import MasterState, IntentOutput
from app.agents.buyer.config import llm

# Import Subgraphs đã compile
from app.agents.buyer.subgraphs.general import general_graph_app
from app.agents.buyer.subgraphs.product import product_graph_app
from app.agents.buyer.subgraphs.order import order_graph_app
from app.agents.buyer.subgraphs.policy import policy_graph_app

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
    You are an intent classification engine for a production e-commerce AI system.
    Your ONLY task is to classify the user's LATEST message into ONE of the predefined intent categories based on the conversation context.

    ---------------------------------------
    CONVERSATION CONTEXT (Recent History):
    {history_text}
    ---------------------------------------

    ---------------------------------------
    INTENT CATEGORIES
    ---------------------------------------

    1. "product_search"
    - User is looking for products.
    - Examples:
        - "I want black nike shoes under $100"
        - "Show me gaming laptops"
        - "Do you have waterproof jackets?"
        - "Find me a keyboard"
        - "Do you have any shirts"

    2. "order_tracking"
    - User is asking about a specific order.
    - Includes:
        - Order status
        - Order tracking
        - Delivery updates
        - Order cancellation
        - Refund status
    - Examples:
        - "Where is my order?"
        - "Track order #12345"
        - "Has my package shipped?"
        - "I want to cancel my order"
        - "I just cancle my newest order, is it succesfully canceled"
        - "Is my newest order on shipping"

    3. "policy_question"
    - User is asking about store policies or company rules.
    - Includes:
        - Return policy
        - Shipping policy
        - Refund policy
        - Payment methods
        - Privacy policy
        - Warranty information
    - Examples:
        - "What is your return policy?"
        - "How long does shipping take?"
        - "Do you accept visa card?"
        - "How can I get help with technical issue"
        - "Is my personal information privately protected"
    4. "general"
    - Greetings, small talk, compliments, or unrelated questions.
    - Examples:
        - "Hi"
        - "Thanks"
        - "You're helpful"
        - "What’s your name?"

    CRITICAL RULE:
    If the latest message is a short follow-up (e.g., "what about blue?", "cheaper", "how much is it?"), you MUST infer the intent from the CONVERSATION CONTEXT.
    Example: 
    - Context: User asked for jackets -> AI recommended jackets.
    - Latest message: "do you have them in black?"
    - Intent: "product_search"

    ---------------------------------------
    INTENT CATEGORIES
    ---------------------------------------
    1. "product_search": Looking for products, specific items, or modifying a previous product search.
    2. "order_tracking": Asking about order status, shipping updates, cancellations, or refunds for a specific order.
    3. "policy_question": Asking about store policies, rules, returns, payments, or general support.
    4. "general": Greetings, small talk, compliments, or unrelated questions.    
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
    user_id = "1"
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