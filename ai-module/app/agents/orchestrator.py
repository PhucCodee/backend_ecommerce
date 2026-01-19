from langgraph.graph import StateGraph, START, END
from pydantic import BaseModel, Literal
from app.core.config import get_llm
from app.core.state import MasterState
from app.agents.prompts import ROUTER_SYSTEM_PROMPT

# Import Sub-Graphs
from app.agents.sql_agent import sql_graph
from app.agents.rag_agent import rag_graph # (Assuming you refactor rag_agent similarly)

llm = get_llm()

class RouterOutput(BaseModel):
    intent: Literal["policy", "database", "general"]

def router_node(state: MasterState):
    """Decides where to send the user."""
    structured_llm = llm.with_structured_output(RouterOutput)
    last_msg = state["messages"][-1]
    
    response = structured_llm.invoke([
        {"role": "system", "content": ROUTER_SYSTEM_PROMPT},
        {"role": "user", "content": last_msg.content}
    ])
    return {"next_step": response.intent}

def call_general_chat(state: MasterState):
    msg = llm.invoke([{"role":"system", "content":"Be nice."}, state["messages"][-1]])
    return {"messages": [msg]}

# Main Graph Construction
workflow = StateGraph(MasterState)

workflow.add_node("router", router_node)
workflow.add_node("sql_branch", sql_graph)
workflow.add_node("rag_branch", rag_graph) 
workflow.add_node("general_branch", call_general_chat)

workflow.add_edge(START, "router")

workflow.add_conditional_edges(
    "router",
    lambda x: x["next_step"],
    {
        "database": "sql_branch",
        "policy": "rag_branch",
        "general": "general_branch"
    }
)

workflow.add_edge("sql_branch", END)
workflow.add_edge("rag_branch", END)
workflow.add_edge("general_branch", END)

# Export the compiled graph
orchestrator_app = workflow.compile()