# app/agents/buyer/subgraphs/policy.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, ToolMessage, AIMessage
from langchain_core.tools import tool
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from app.agents.state import MasterState, PolicyAction
from app.agents.config import llm
from pathlib import Path
import os
import json
from langchain_chroma import Chroma
from dotenv import load_dotenv
import json

load_dotenv()

embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")

default_path = Path(__file__).resolve().parents[3] / "data" / "vector" / "chroma_db_data1"
persist_directory = os.getenv("CHROMA_DB_PATH", str(default_path))

# ── Policy vectorstore (giữ nguyên) ──────────────────────────────────────────
vectorstore = Chroma(
    persist_directory=str(persist_directory),
    collection_name="rag_document",
    embedding_function=embeddings,
)
retriever = vectorstore.as_retriever(search_type="similarity", search_kwargs={"k": 5})

# ── FAQ vectorstore (mới) ─────────────────────────────────────────────────────
faq_vectorstore = Chroma(
    persist_directory=str(persist_directory),
    collection_name="faq_document",
    embedding_function=embeddings,
)

FAQ_SIMILARITY_THRESHOLD = 0.85   # tune nếu cần
FAQ_TOP_K = 3






# ==========================================
# 1. KHAI BÁO CÔNG CỤ (TOOLS)  — giữ nguyên
# ==========================================
@tool
def retriever_tool(query: str) -> str:
    """
    Use this tool to find information about "Sanquo" e-commerce store policies.
    This tool is the ONLY source for information on:
    _ Introduction (information about the store, its features, and how it works)
    _ Conditions of use (rules and guidelines for using the store)
    _ Payment methods (available payment options and instructions)
    _ Shipping and delivery (shipping options, delivery times, and related policies)
    _ Return and refund policies (conditions for returns, refunds, and exchanges)
    _ Privacy note (how user data is handled and protected)
    _ Contact information (how to reach customer support for policy-related questions)
    _ Technical issues (troubleshooting common problems related to policies or store features)
    """
    print(f"Calling retriever_tool with query: {query}")
    docs = retriever.invoke(query)

    print(f"--> [DEBUG] Found {len(docs)} documents in ChromaDB.")
    if not docs:
        print("--> [DEBUG] No documents found!")
        return json.dumps({"contexts": [], "message": "No relevant information found in the store policy documents."})

    contexts = []
    for doc in docs:
        source_doc = doc.metadata.get("source_document", "Unknown")
        page = doc.metadata.get("page", "N/A")
        content = doc.page_content
        print(f"--> [DEBUG] Retrieved snippet: {content[:100]}...")
        contexts.append({"content": content, "source": {"document": source_doc, "page": page}})

    readable_parts = []
    for idx, ctx in enumerate(contexts, 1):
        readable_parts.append(
            f"[{idx}] Source: {ctx['source']['document']} (Page {ctx['source']['page']})\n"
            f"Content: {ctx['content']}"
        )

    return json.dumps({"contexts": contexts, "readable": "\n\n".join(readable_parts)})


tools = [retriever_tool]
tool_llm = llm.bind_tools(tools)
tools_dict = {our_tool.name: our_tool for our_tool in tools}


# ==========================================
# 2. KHAI BÁO CÁC NODE
# ==========================================
def policy_faq(state: MasterState):
    print("\n--- 🔀 Routing to Policy FAQ Agent ---")
    action = PolicyAction(human_mes=state["user_prompt"])
    return {"log_action": [action]}


# ── NODE MỚI: tìm FAQ trước ───────────────────────────────────────────────────
def faq_lookup(state: MasterState):
    """
    Tìm kiếm trong FAQ collection. 
    Nếu độ tương đồng (score) >= 0.85, lấy trực tiếp câu trả lời từ answer_data.
    """
    query = state["user_prompt"]
    print(f"\n--- 🔍 FAQ Lookup: '{query}' ---")

    # Thực hiện search
    hits = faq_vectorstore.similarity_search_with_relevance_scores(query, k=FAQ_TOP_K)

    if not hits:
        print("--> [FAQ] No hits found, falling through to LLM.")
        return {"faq_hit": False}

    best_doc, best_score = hits[0]
    print(f"--> [FAQ] Best score: {best_score:.4f} (Required: >={FAQ_SIMILARITY_THRESHOLD})")

    # Kiểm tra ngưỡng 0.85
    if best_score < FAQ_SIMILARITY_THRESHOLD:
        print(f"--> [FAQ] Score {best_score:.4f} is too low. Falling through to LLM.")
        return {"faq_hit": False}

    # Gom các câu trả lời đạt chuẩn
    answer_parts = []
    for doc, score in hits:
        if score >= FAQ_SIMILARITY_THRESHOLD:
            # Lấy faq_id từ metadata của vector vừa hit
            faq_id = doc.metadata.get("id") # Hoặc "faq_id" tùy bạn đặt lúc index
            
            # Truy xuất từ dictionary answer_data bạn vừa tạo ở bước trước
            actual_answer = answer_map.get(faq_id)
            
            if actual_answer:
                answer_parts.append(actual_answer)
                print(f"--> [FAQ] ✅ Hit found! ID: {faq_id} | Score: {score:.4f}")
            else:
                print(f"--> [FAQ] ⚠️ Found ID {faq_id} in vectorstore but not in answer_data JSON!")

    if not answer_parts:
        return {"faq_hit": False}

    # Nối các câu trả lời nếu có nhiều hit (thường chỉ có 1)
    answer_text = "\n\n".join(answer_parts)
    print("--> [FAQ] Direct answer retrieved. Skipping LLM call.")

    return {
        "faq_hit": True,
        "messages": [AIMessage(content=answer_text)],
    }


def route_after_faq_lookup(state: MasterState):
    """Router sau faq_lookup."""
    if state.get("faq_hit"):
        return "faq_synthesize"
    return "call_llm"


# ── Các node giữ nguyên ───────────────────────────────────────────────────────
def call_llm(state: MasterState):
    prompt = """You are a helpful AI assistant for "Sanquo", an e-commerce platform.
    Your primary role is to answer customer questions about the store's policies and features.

    **Core Rules:**
    1. You **MUST** use the `retriever_tool`.
    2. **STOP SEARCHING IMMEDIATELY** once you have found relevant information in the tool output. Do not re-query with different keywords if you already have the answer.
    3. Answer **ONLY** based on the information provided by the tool. Use the "readable" field in the tool output for your answer.
    4. If the tool returns no contexts, clearly state that and **STOP**.
    5. You should not make up any information that is not present in the tool output. If you don't know or no information related, say you don't know. Focus on enhancing Contextual Relevance , Faithfulness,  Answer Relevancy.
    """
    messages = [SystemMessage(content=prompt)] + state["messages"]
    response = tool_llm.invoke(messages)
    return {"messages": [response]}


def find_context(state: MasterState):
    tool_calls = state["messages"][-1].tool_calls
    results = []
    all_contexts = []

    for t in tool_calls:
        if t["name"] not in tools_dict:
            raw_result = json.dumps({"contexts": [], "message": "Invalid tool name."})
        else:
            raw_result = tools_dict[t["name"]].invoke(t["args"].get("query", ""))

        try:
            parsed = json.loads(raw_result) if isinstance(raw_result, str) else raw_result
            if isinstance(parsed, dict) and "contexts" in parsed:
                all_contexts.extend(parsed["contexts"])
                llm_content = parsed.get("readable", raw_result)
            else:
                llm_content = str(raw_result)
        except (json.JSONDecodeError, TypeError):
            llm_content = str(raw_result)

        results.append(
            ToolMessage(tool_call_id=t["id"], name=t["name"], content=llm_content)
        )

    print("Tools Execution Complete. Back to the model!")

    seen = set()
    unique_contexts = []
    for ctx in all_contexts:
        key = (ctx["source"]["document"], str(ctx["source"]["page"]))
        if key not in seen:
            seen.add(key)
            unique_contexts.append(ctx)

    existing_ui_data = state.get("ui_data") or {}
    updated_ui_data = {**existing_ui_data, "policy_contexts": unique_contexts}

    return {"messages": results, "ui_data": updated_ui_data}


def should_continue(state: MasterState):
    last_message = state["messages"][-1]
    if hasattr(last_message, "tool_calls") and len(last_message.tool_calls) > 0:
        return "find_context"
    return "faq_synthesize"


def faq_synthesize(state: MasterState):
    ai_answer = state["messages"][-1]
    answer_text = ai_answer.content

    existing_ui_data = state.get("ui_data") or {}
    raw_contexts: list = existing_ui_data.pop("policy_contexts", [])

    # Nếu đến từ faq_lookup thì không có policy_contexts — context rỗng là đúng
    final_ui_data = {**existing_ui_data, "Context": raw_contexts}

    return {"answer": answer_text, "ui_data": final_ui_data}


# ==========================================
# 3. BUILD GRAPH
# ==========================================
policy_workflow = StateGraph(MasterState)

policy_workflow.add_node("policy_faq",    policy_faq)
# policy_workflow.add_node("faq_lookup",    faq_lookup)      
policy_workflow.add_node("call_llm",      call_llm)
policy_workflow.add_node("find_context",  find_context)
policy_workflow.add_node("faq_synthesize", faq_synthesize)

policy_workflow.add_edge(START,        "policy_faq")
policy_workflow.add_edge("policy_faq", "call_llm")       

# policy_workflow.add_conditional_edges(
#     "faq_lookup",
#     route_after_faq_lookup,
#     {
#         "faq_synthesize": "faq_synthesize",   # FAQ hit ✅ → bỏ qua LLM
#         "call_llm":       "call_llm",          # FAQ miss  → LLM + policy RAG
#     },
# )

policy_workflow.add_conditional_edges(
    "call_llm",
    should_continue,
    {
        "find_context":   "find_context",
        "faq_synthesize": "faq_synthesize",
    },
)

policy_workflow.add_edge("find_context",  "call_llm")
policy_workflow.add_edge("faq_synthesize", END)

policy_graph_app = policy_workflow.compile()