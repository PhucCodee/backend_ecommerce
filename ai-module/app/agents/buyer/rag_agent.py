# rag_agent.py

from dotenv import load_dotenv
import os
from langgraph.graph import StateGraph, END
# Thêm import GraphRecursionError để bắt lỗi vòng lặp
from langgraph.errors import GraphRecursionError 
from typing import TypedDict, Annotated, Sequence
from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage, ToolMessage
from operator import add as add_messages
from langchain_chroma import Chroma
from langchain_core.tools import tool
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain.chat_models import init_chat_model
from pathlib import Path

load_dotenv()

# Make sure your GOOGLE_API_KEY is set in your .env or environment
embeddings = GoogleGenerativeAIEmbeddings(
    model="models/gemini-embedding-001",
)

llm = init_chat_model(
    "llama-3.3-70b-versatile", 
    model_provider="groq" ,
    temperature = 0, 
)

default_path = Path(__file__).resolve().parents[3] / "data" / "vector"
persist_directory = os.getenv("CHROMA_DB_PATH", str(default_path))


vectorstore = Chroma(
    persist_directory=str(persist_directory), # Chuyển Path thành string cho chắc chắn
    collection_name="rag_document",
    embedding_function=embeddings  
)

retriever = vectorstore.as_retriever(
    search_type="similarity",
    search_kwargs={"k": 5} 
)

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
    print(f"Path: {persist_directory}")

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

llm = llm.bind_tools(tools)

class AgentState(TypedDict):
    messages: Annotated[Sequence[BaseMessage], add_messages]


def should_continue(state: AgentState):
    """Check if the last message contains tool calls."""
    result = state['messages'][-1]
    return hasattr(result, 'tool_calls') and len(result.tool_calls) > 0

# Đã cập nhật System Prompt chặt chẽ hơn để ngăn AI tự ý search lại
system_prompt = """
You are a helpful AI assistant for "My Shop", an e-commerce platform.
Your primary role is to answer customer questions about the store's policies and features.

**Core Rules:**
1. For **all** questions related to store policies, you **MUST** use the `retriever_tool`.
2. **STOP SEARCHING IMMEDIATELY** once you have found relevant information in the tool output. Do not re-query with different keywords if you already have the answer.
3. Answer **ONLY** based on the information provided by the tool.
4. If the tool returns "No relevant information found...", clearly state that and **STOP**.
5. **Citation is mandatory.**

**Citation Example:**
"You can cancel an order as long as it has not yet been shipped (Source: Policy Doc, Page 2)."
"""


tools_dict = {our_tool.name: our_tool for our_tool in tools} 

# LLM Agent
def call_llm(state: AgentState) -> AgentState:
    """Function to call the LLM with the current state."""
    messages = list(state['messages'])
    messages = [SystemMessage(content=system_prompt)] + messages
    message = llm.invoke(messages)
    return {'messages': [message]}


# Retriever Agent
def take_action(state: AgentState) -> AgentState:
    """Execute tool calls from the LLM's response."""
    tool_calls = state['messages'][-1].tool_calls
    results = []
    for t in tool_calls:
        print(f"Calling Tool: {t['name']} with query: {t['args'].get('query', 'No query provided')}")
        
        if not t['name'] in tools_dict: 
            print(f"\nTool: {t['name']} does not exist.")
            result = "Incorrect Tool Name, Please Retry and Select tool from List of Available tools."
        else:
            result = tools_dict[t['name']].invoke(t['args'].get('query', ''))
            print(f"Result length: {len(str(result))}")
            
        results.append(ToolMessage(tool_call_id=t['id'], name=t['name'], content=str(result)))

    print("Tools Execution Complete. Back to the model!")
    return {'messages': results}


graph = StateGraph(AgentState)
graph.add_node("llm", call_llm)
graph.add_node("retriever_agent", take_action)

graph.add_conditional_edges(
    "llm",
    should_continue,
    {True: "retriever_agent", False: END}
)
graph.add_edge("retriever_agent", "llm")
graph.set_entry_point("llm")

rag_agent = graph.compile()


def running_agent():
    print("\n=== RAG AGENT===")
    
    while True:
        user_input = input("\nWhat is your question: ")
        if user_input.lower() in ['exit', 'quit']:
            break
            
        messages = [HumanMessage(content=user_input)] 

        # --- SỬA Ở ĐÂY: Thêm try/except và recursion_limit ---
        try:
            result = rag_agent.invoke(
                {"messages": messages},
                config={"recursion_limit": 5} # Giới hạn tối đa 5 bước nhảy (LLM -> Tool -> LLM -> Tool -> Stop)
            )
            
            print("\n=== ANSWER ===")
            print(result['messages'][-1].content)
            
        except GraphRecursionError:
            print("\n=== SYSTEM NOTICE ===")
            print("⚠️ Agent đã dừng do vượt quá số lần tìm kiếm cho phép (Infinite Loop Prevention).")
            print("AI không thể tìm thấy câu trả lời dứt khoát hoặc bị kẹt trong vòng lặp.")
        except Exception as e:
             print(f"\n❌ Đã xảy ra lỗi hệ thống: {e}")


# Phần này giữ nguyên như yêu cầu của bạn
if __name__ == "__main__":
    print(persist_directory)
    # Lưu ý: Nếu persist_directory sai, dòng này sẽ crash hoặc trả về 0
    try:
        count = vectorstore._collection.count()
        print(f"Loaded VectorStore with {count} documents.")
    except Exception as e:
        print(f"Error loading vectorstore: {e}")
        
    running_agent()