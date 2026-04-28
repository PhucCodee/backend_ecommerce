# app/agents/subgraphs/general.py
from langgraph.graph import StateGraph, START, END
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage

# Import từ thư mục core/cấu hình của bạn
from app.agents.state import MasterState, GeneralAction, GeneralUIResponse
from app.agents.config import llm

def general_agent(state: MasterState):
    print("\n--- 🔀 Routing to General Chat Agent ---")
    
    # 1. Trích xuất ngữ cảnh (lịch sử chat gần nhất) để bot chat tự nhiên hơn
    chat_history = [
        msg for msg in state["messages"] 
        if isinstance(msg, (HumanMessage, AIMessage))
    ]
    # Lấy tối đa 5 tin nhắn gần nhất để làm ngữ cảnh
    recent_history = chat_history[-5:] 
    
    # 2. Xây dựng System Prompt
    system_prompt = """You are a helpful, friendly, and concise e-commerce assistant for "My Shop". 
    Your task is to handle general greetings, small talk, compliments, or unrelated questions politely.
    If the user asks something completely outside the scope of e-commerce, gently steer them back to shopping or asking about store policies.
    Keep your response natural and conversational (2-3 sentences max).
    """
    
    messages_to_send = [SystemMessage(content=system_prompt)] + recent_history
    
    # Nếu recent_history chưa có câu hỏi hiện tại (do cách setup ở ngoài), ta dự phòng nối thêm:
    if not recent_history or recent_history[-1].content != state["user_prompt"]:
        messages_to_send.append(HumanMessage(content=state["user_prompt"]))

    # 3. Gọi LLM
    msg = llm.invoke(messages_to_send)
    
    # 4. Ghi log hành động
    action = GeneralAction(
        human_mes=state["user_prompt"],
        ai_ans=msg.content
    )
    
    # 5. Cập nhật MasterState với format consistent
    # Trả về AIMessage để LangGraph tự động nối vào lịch sử hội thoại
    return {
        "answer": msg.content,
        "ui_data": {},  # 🔄 Ensure ui_data is always present (empty for general)
        "messages": [AIMessage(content=msg.content)],
        "log_action": [action]
    }

# ==========================================
# KHỞI TẠO VÀ BUILD SUBGRAPH
# ==========================================
general_workflow = StateGraph(MasterState)

# Add Node
general_workflow.add_node("general_agent", general_agent)

# Nối Edge
general_workflow.add_edge(START, "general_agent")
general_workflow.add_edge("general_agent", END)

# Compile & Export
general_graph_app = general_workflow.compile()