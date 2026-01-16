import uvicorn
import uuid
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from langchain_core.messages import HumanMessage

# Import compiled graph từ orchestrator.py
from orchestrator import app as orchestrator_app

api = FastAPI(
    title="E-Commerce AI Chatbot API",
    description="Demo version: Mỗi request là một phiên độc lập.",
    version="1.0"
)

# --- CORS ---
api.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # Cho phép tất cả để dễ dàng test demo
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class ChatRequest(BaseModel):
    message: str

class ChatResponse(BaseModel):
    response: str

@api.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest):
    try:
        # Tự động tạo một thread_id ngẫu nhiên cho mỗi request 
        # Điều này giúp mỗi câu hỏi của user là một phiên mới (Clean state)
        session_id = str(uuid.uuid4())
        config = {"configurable": {"thread_id": session_id}}
        
        # Chuẩn bị input
        initial_state = {"messages": [HumanMessage(content=request.message)]}
        
        # Chạy orchestrator
        # Lưu ý: Dùng ainvoke vì FastAPI là async, giúp tránh chặn event loop
        result = await orchestrator_app.ainvoke(initial_state, config=config)
        
        # Lấy tin nhắn cuối cùng từ danh sách messages
        ai_message = result["messages"][-1].content
        
        return ChatResponse(response=ai_message)

    except Exception as e:
        print(f"❌ Error processing request: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    print("🚀 FastAPI Server đang chạy tại http://0.0.0.0:8000")
    uvicorn.run(api, host="0.0.0.0", port=8000)