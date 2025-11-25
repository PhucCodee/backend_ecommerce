import os
from dotenv import load_dotenv
from typing import Annotated, Literal, List, Dict, Any
from typing_extensions import TypedDict

from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from langchain_core.messages import HumanMessage, BaseMessage
from langchain.chat_models import init_chat_model
from pydantic import BaseModel, Field

# --- Import your compiled graphs ---
# Make sure sql_agent.py and rag_agent.py represent the filenames exactly
from sql_agent import sql_graph
from rag_agent import rag_agent as rag_graph 

load_dotenv()

# --- 1. Init Router LLM (Independent Instance) ---
if not os.environ.get("GOOGLE_API_KEY"):
    print("Warning: GOOGLE_API_KEY not found.")

llm = init_chat_model(
    "gemini-2.5-flash", 
    model_provider="google_genai", 
    temperature=0
)

# --- 2. Define Master State ---
# This state must include ALL keys required by ALL sub-agents.
# SQL Agent uses: sql_query, sql_result
# RAG Agent uses: messages
class MasterState(TypedDict):
    messages: Annotated[list, add_messages]
    # Keys for SQL Agent to function
    sql_query: str | None
    sql_result: List[Dict[str, Any]] | str | None
    final_answer: str | None
    # Key for Router decision
    next_step: str | None

# --- 3. The Router (Classifier) ---
class RouterOutput(BaseModel):
    """Classify user intent."""
    intent: Literal["policy", "database", "general"] = Field(
        ...,
        description="The appropriate expert to handle the query."
    )

def router_node(state: MasterState):
    """Classifies the intent of the last message."""
    messages = state["messages"]
    last_user_msg = messages[-1]
    
    structured_llm = llm.with_structured_output(RouterOutput)
    
    system_msg = """You are the Router for an E-commerce AI system.
    Route the query to the correct expert:
    
    - 'database': For queries about specific products, prices, stock levels, orders, or user data. (Requires SQL access).
    - 'policy': For general questions about shipping, returns, company info, privacy, or "how to" guides. (Requires Document search).
    - 'general': For greetings, compliments, or off-topic conversation.
    """
    
    response = structured_llm.invoke([
        {"role": "system", "content": system_msg},
        {"role": "user", "content": last_user_msg.content}
    ])
    
    return {"next_step": response.intent}

# --- 4. Subgraph Wrappers ---

def call_sql_agent(state: MasterState):
    print("\n--- 🔀 Routing to SQL Agent ---")
    # We invoke the compiled SQL graph with the current state
    result = sql_graph.invoke(state)
    # The result contains the updated state from the SQL agent
    return result

def call_rag_agent(state: MasterState):
    print("\n--- 🔀 Routing to Policy (RAG) Agent ---")
    result = rag_graph.invoke(state)
    return result

def call_general_agent(state: MasterState):
    print("\n--- 🔀 Routing to General Chat ---")
    # Simple direct response for greetings
    msg = llm.invoke([
        {"role": "system", "content": "You are a helpful e-commerce assistant. Respond politely and concisely."},
        state["messages"][-1]
    ])
    return {"messages": [msg]}

# --- 5. Build the Master Graph ---
workflow = StateGraph(MasterState)

workflow.add_node("router", router_node)
workflow.add_node("sql_branch", call_sql_agent)
workflow.add_node("rag_branch", call_rag_agent)
workflow.add_node("general_branch", call_general_agent)

workflow.add_edge(START, "router")

def route_decision(state: MasterState):
    return state["next_step"]

workflow.add_conditional_edges(
    "router",
    route_decision,
    {
        "database": "sql_branch",
        "policy": "rag_branch",
        "general": "general_branch"
    }
)

workflow.add_edge("sql_branch", END)
workflow.add_edge("rag_branch", END)
workflow.add_edge("general_branch", END)

app = workflow.compile()

# --- 6. Main Execution Loop ---
if __name__ == "__main__":
    print("=== ORCHESTRATOR AGENT STARTED ===")
    print("Type 'exit' to quit.\n")
    
    while True:
        try:
            user_input = input("User: ")
            if user_input.lower() in ["exit", "quit"]:
                break
            
            # Initialize state with the new user message
            initial_state = {"messages": [HumanMessage(content=user_input)]}
            
            # Run the graph
            result = app.invoke(initial_state)
            
            # Extract the final response
            # The 'messages' list accumulates history; the last one is the AI response.
            final_msg = result["messages"][-1].content
            print(f"Assistant: {final_msg}\n")
            
        except Exception as e:
            print(f"An error occurred: {e}")