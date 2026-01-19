# app/agents/sql_agent.py
from typing import Dict, Any
from langgraph.graph import StateGraph, START, END
from pydantic import BaseModel, Field
from langchain_core.messages import AIMessage

# Internal Imports
from app.core.config import get_llm
from app.core.state import MasterState
from app.agents.prompts import SQL_SYSTEM_PROMPT, SQL_SYNTHESIS_PROMPT
from app.tools.database import execute_sql_tool

# 1. Setup
llm = get_llm()

# 2. Structured Output Definition
class SQLQueryOutput(BaseModel):
    query: str = Field(..., description="The PostgreSQL query to execute.")
    explanation: str = Field(..., description="Brief logic explanation.")

# 3. Nodes
def generate_query_node(state: MasterState):
    """Step 1: Convert natural language to SQL."""
    structured_llm = llm.with_structured_output(SQLQueryOutput)
    last_message = state["messages"][-1]
    
    result = structured_llm.invoke([
        {"role": "system", "content": SQL_SYSTEM_PROMPT},
        {"role": "user", "content": last_message.content}
    ])
    
    # Update state with the generated query
    return {"sql_query": result.query}

def execute_query_node(state: MasterState):
    """Step 2: Run the SQL on the DB."""
    query = state["sql_query"]
    # Uses the tool we created earlier
    results = execute_sql_tool(query)
    return {"sql_result": results}

def synthesize_answer_node(state: MasterState):
    """Step 3: Humanize the results."""
    query = state["sql_query"]
    data = state["sql_result"]
    original_question = state["messages"][-1].content
    
    user_prompt = f"""
    Question: {original_question}
    SQL Run: {query}
    Data Results: {data}
    """
    
    response = llm.invoke([
        {"role": "system", "content": SQL_SYNTHESIS_PROMPT},
        {"role": "user", "content": user_prompt}
    ])
    
    return {"messages": [response]}

# 4. Graph Construction
builder = StateGraph(MasterState)

builder.add_node("generate_query", generate_query_node)
builder.add_node("execute_query", execute_query_node)
builder.add_node("synthesize_answer", synthesize_answer_node)

# Linear Flow
builder.add_edge(START, "generate_query")
builder.add_edge("generate_query", "execute_query")
builder.add_edge("execute_query", "synthesize_answer")
builder.add_edge("synthesize_answer", END)

sql_graph = builder.compile()