# app/agents/subgraphs/policy.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, ToolMessage, AIMessage
from langchain_core.tools import tool
import re

# Import từ thư mục core/cấu hình của bạn
from app.agents.state import MasterState, PolicyAction
from app.agents.config import llm, retriever

# ==========================================
# 1. KHAI BÁO CÔNG CỤ (TOOLS)
# ==========================================
@tool
def retriever_tool(query: str) -> str:
    """
    Use this tool to find information about "My Shop" e-commerce store policies.
    This tool is the ONLY source for information on:
    - Return policy, cancellations, and refunds
    - Shipping and delivery information
    - Payment methods
    - Privacy notice and Conditions of Use
    - How to contact customer support
    - Technical issues or device support
    - Introduction to the store
    """
    print(f"Calling retriever_tool with query: {query}")
    docs = retriever.invoke(query) 

    if not docs:
        return "No relevant information found in the store policy documents."
    
    results = []
    for doc in docs:
        source = doc.metadata.get('source_document', 'Unknown')
        page = doc.metadata.get('page', 'N/A')
        results.append(f"Source: {source} (Page {page})\nContent: {doc.page_content}")
    
    return "\n\n".join(results)

tools = [retriever_tool]
tool_llm = llm.bind_tools(tools)
tools_dict = {our_tool.name: our_tool for our_tool in tools} 

# ==========================================
# 2. KHAI BÁO CÁC NODE (BƯỚC CHẠY)
# ==========================================
def policy_faq(state: MasterState):
    print("\n--- 🔀 Routing to Policy FAQ Agent ---")
    action = PolicyAction(human_mes=state["user_prompt"])
    return {"log_action": [action]}

def call_llm(state: MasterState):
    """Function to call the LLM with the current state."""
    prompt = """You are a helpful AI assistant for "My Shop", an e-commerce platform.
    Your primary role is to answer customer questions about the store's policies and features.

    **Core Rules:**
    1. For **all** questions related to store policies, you **MUST** use the `retriever_tool`.
    2. **STOP SEARCHING IMMEDIATELY** once you have found relevant information in the tool output. Do not re-query with different keywords if you already have the answer.
    3. Answer **ONLY** based on the information provided by the tool.
    4. If the tool returns "No relevant information found...", clearly state that and **STOP**.
    5. **Citation is mandatory.** Example: "You can cancel an order... (Source: Policy Doc, Page 2)."
    """
    
    # Kết hợp system prompt và lịch sử hội thoại
    messages = [SystemMessage(content=prompt)] + state['messages']
    response = tool_llm.invoke(messages)

    # Trả về AIMessage hoặc AIMessage có chứa tool_calls
    # LangGraph sẽ tự append response này vào mảng state['messages']
    return {'messages': [response]}

def find_context(state: MasterState):
    """Execute tool calls from the LLM's response."""
    tool_calls = state['messages'][-1].tool_calls
    results = []
    
    for t in tool_calls:
        if t["name"] not in tools_dict:
            result = "Invalid tool name."
        else:
            result = tools_dict[t["name"]].invoke(t["args"].get("query", ""))

        results.append(
            ToolMessage(
                tool_call_id=t["id"],
                name=t["name"],
                content=str(result)
            )
        )

    print("Tools Execution Complete. Back to the model!")
    return {'messages': results}

def should_continue(state: MasterState):
    """Check if the last message contains tool calls."""
    last_message = state['messages'][-1]
    if hasattr(last_message, 'tool_calls') and len(last_message.tool_calls) > 0:
        return "find_context"
    return "faq_synthesize"

def faq_synthesize(state: MasterState):
    """Extract final answer and structure response consistently."""
    ai_answer = state['messages'][-1]
    
    # Extract sources from message content for ui_data
    sources = []
    if isinstance(ai_answer, AIMessage):
        # Look for source citations in the format "Source: filename (Page X)"
        source_pattern = r'Source:\s*([^\(]+)\s*\(Page\s*([^\)]+)\)'
        matches = re.findall(source_pattern, ai_answer.content)
        sources = [{"source": match[0].strip(), "page": match[1].strip()} for match in matches]
    
    # 🔄 Return consistent format with ui_data and AIMessage
    return {
        'answer': ai_answer.content,
        'ui_data': {'sources': sources},
        'messages': [AIMessage(content=ai_answer.content)] if not isinstance(ai_answer, AIMessage) else []
    }

# ==========================================
# 3. KHỞI TẠO VÀ BUILD SUBGRAPH
# ==========================================
policy_workflow = StateGraph(MasterState)

# Add nodes
policy_workflow.add_node("policy_faq", policy_faq)
policy_workflow.add_node("call_llm", call_llm)
policy_workflow.add_node("find_context", find_context)
policy_workflow.add_node("faq_synthesize", faq_synthesize)

# Define routing
policy_workflow.add_edge(START, "policy_faq")
policy_workflow.add_edge("policy_faq", "call_llm")

# Node call_llm sẽ tự quyết định đi tìm context hay đi tổng hợp kết quả
policy_workflow.add_conditional_edges(
    "call_llm",
    should_continue,
    {
        "find_context": "find_context",
        "faq_synthesize": "faq_synthesize"
    }
)

policy_workflow.add_edge("find_context", "call_llm")
policy_workflow.add_edge("faq_synthesize", END)

# Compile & Export
policy_graph_app = policy_workflow.compile()