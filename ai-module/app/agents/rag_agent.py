# app/agents/rag_agent.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage

# Internal Imports
from app.core.config import get_llm
from app.core.state import MasterState
from app.agents.prompts import RAG_SYSTEM_PROMPT
from app.tools.vector_store import retrieve_documents

# 1. Setup
llm = get_llm()

# 2. Nodes
def retrieve_node(state: MasterState):
    """Step 1: Look up the policy in the Vector DB."""
    last_message = state["messages"][-1]
    query = last_message.content
    
    # Retrieve context from our helper tool
    context_text = retrieve_documents(query)
    
    # We save this temporarily in the state (or just pass it to the next node via returns)
    # Since MasterState doesn't have a 'context' key, we pass it implicitly 
    # or we can update the prompt immediately.
    return {"rag_context": context_text} 
    # Note: If 'rag_context' isn't in MasterState, we should add it OR handle it inside one node.
    # To keep MasterState clean, let's combine Retrieve + Generate into one smart node 
    # OR update MasterState in app/core/state.py to include 'rag_context'.
    
    # For now, let's do the CLEANEST approach: Combine retrieval and generation here
    # to avoid changing MasterState.

def rag_pipeline_node(state: MasterState):
    """
    Performs both Retrieval and Generation in one logical step 
    to keep the graph simple.
    """
    last_message = state["messages"][-1]
    query = last_message.content
    
    # 1. Retrieve
    print(f"--- 🔍 Searching docs for: {query} ---")
    context = retrieve_documents(query)
    
    if not context:
        context = "No specific policy documents found."

    # 2. Generate
    # Inject context into the system prompt
    final_system_prompt = RAG_SYSTEM_PROMPT.format(context=context)
    
    response = llm.invoke([
        {"role": "system", "content": final_system_prompt},
        {"role": "user", "content": query}
    ])
    
    return {"messages": [response]}

# 3. Graph Construction
builder = StateGraph(MasterState)

builder.add_node("rag_pipeline", rag_pipeline_node)

builder.add_edge(START, "rag_pipeline")
builder.add_edge("rag_pipeline", END)

rag_graph = builder.compile()