# app/agents/buyer/subgraphs/policy.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, ToolMessage
from langchain_core.tools import tool
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from app.agents.state import MasterState, PolicyAction
from app.agents.config import llm
from pathlib import Path
import os
import json
from langchain_chroma import Chroma
from dotenv import load_dotenv

load_dotenv()

embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")

default_path = Path(__file__).resolve().parents[3] / "data" / "vector" / "chroma_db_data1"
persist_directory = os.getenv("CHROMA_DB_PATH", str(default_path))

vectorstore = Chroma(
    persist_directory=str(persist_directory),
    collection_name="rag_document",
    embedding_function=embeddings  
)

retriever = vectorstore.as_retriever(search_type="similarity", search_kwargs={"k": 5})

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
    
    print(f"--> [DEBUG] Found {len(docs)} documents in ChromaDB.")
    if not docs:
        print("--> [DEBUG] No documents found!")
        # Return JSON so faq_synthesize can always parse consistently
        return json.dumps({"contexts": [], "message": "No relevant information found in the store policy documents."})
    
    contexts = []
    for doc in docs:
        source_doc = doc.metadata.get('source_document', 'Unknown')
        page = doc.metadata.get('page', 'N/A')
        content = doc.page_content
        print(f"--> [DEBUG] Retrieved snippet: {content[:100]}...")
        contexts.append({
            "content": content,
            "source": {
                "document": source_doc,
                "page": page
            }
        })
    
    # Return structured JSON — LLM will see the content, faq_synthesize will parse sources
    readable_parts = []
    for idx, ctx in enumerate(contexts, 1):
        readable_parts.append(
            f"[{idx}] Source: {ctx['source']['document']} (Page {ctx['source']['page']})\n"
            f"Content: {ctx['content']}"
        )

    return json.dumps({
        "contexts": contexts,
        "readable": "\n\n".join(readable_parts)
    })

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
    3. Answer **ONLY** based on the information provided by the tool. Use the "readable" field in the tool output for your answer.
    4. If the tool returns no contexts, clearly state that and **STOP**.
    5. **Citation is mandatory.** Example: "You can cancel an order... (Source: Policy Doc, Page 2)."
    """
    
    messages = [SystemMessage(content=prompt)] + state['messages']
    response = tool_llm.invoke(messages)
    return {'messages': [response]}

def find_context(state: MasterState):
    """Execute tool calls and store raw structured results for ui_data."""
    tool_calls = state['messages'][-1].tool_calls
    results = []
    all_contexts = []  # Accumulate contexts across all tool calls

    for t in tool_calls:
        if t["name"] not in tools_dict:
            raw_result = json.dumps({"contexts": [], "message": "Invalid tool name."})
        else:
            raw_result = tools_dict[t["name"]].invoke(t["args"].get("query", ""))

        # Parse structured result to collect contexts
        try:
            parsed = json.loads(raw_result) if isinstance(raw_result, str) else raw_result
            if isinstance(parsed, dict) and "contexts" in parsed:
                all_contexts.extend(parsed["contexts"])
                # Feed only the readable part to LLM to keep context window clean
                llm_content = parsed.get("readable", raw_result)
            else:
                llm_content = str(raw_result)
        except (json.JSONDecodeError, TypeError):
            llm_content = str(raw_result)

        results.append(
            ToolMessage(
                tool_call_id=t["id"],
                name=t["name"],
                content=llm_content  # LLM sees human-readable text
            )
        )

    print("Tools Execution Complete. Back to the model!")

    # Deduplicate contexts by (document, page) while preserving order
    seen = set()
    unique_contexts = []
    for ctx in all_contexts:
        key = (ctx["source"]["document"], str(ctx["source"]["page"]))
        if key not in seen:
            seen.add(key)
            unique_contexts.append(ctx)

    # Merge into existing ui_data so other agents' data isn't overwritten
    existing_ui_data = state.get("ui_data") or {}
    updated_ui_data = {
        **existing_ui_data,
        "policy_contexts": unique_contexts  # Temp key; faq_synthesize will finalize
    }

    return {
        'messages': results,
        'ui_data': updated_ui_data
    }

def should_continue(state: MasterState):
    """Check if the last message contains tool calls."""
    last_message = state['messages'][-1]
    if hasattr(last_message, 'tool_calls') and len(last_message.tool_calls) > 0:
        return "find_context"
    return "faq_synthesize"

def faq_synthesize(state: MasterState):
    ai_answer = state['messages'][-1]
    answer_text = ai_answer.content

    existing_ui_data = state.get("ui_data") or {}
    raw_contexts: list = existing_ui_data.pop("policy_contexts", [])

    # source đã nằm trong từng Context item rồi, không cần lặp lại
    final_ui_data = {
        **existing_ui_data,
        "Context": raw_contexts,
    }

    return {
        'answer': answer_text,
        'ui_data': final_ui_data
    }
# ==========================================
# 3. KHỞI TẠO VÀ BUILD SUBGRAPH
# ==========================================
policy_workflow = StateGraph(MasterState)

policy_workflow.add_node("policy_faq", policy_faq)
policy_workflow.add_node("call_llm", call_llm)
policy_workflow.add_node("find_context", find_context)
policy_workflow.add_node("faq_synthesize", faq_synthesize)

policy_workflow.add_edge(START, "policy_faq")
policy_workflow.add_edge("policy_faq", "call_llm")

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

policy_graph_app = policy_workflow.compile()