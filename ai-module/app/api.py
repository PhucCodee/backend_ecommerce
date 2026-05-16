import uvicorn
import json
from typing import Optional, Dict, Any
from fastapi import FastAPI, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr, Field
from langchain_core.messages import HumanMessage

# Import Graph từ module của bạn
from app.agents.agent import app as buyer_orchestrator_app

api = FastAPI(title="AI E-commerce")

# --- MỞ CORS ĐỂ FRONTEND GỌI ĐƯỢC ---
api.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # Môi trường Dev mở *, lên Prod nhớ đưa domain React/Vue vào
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- 1. DATA MODELS ---
class UserContext(BaseModel):
    user_id: int
    username: str
    email: EmailStr
    role: str
    token: Optional[str] = None 
    
    @property
    def is_admin(self) -> bool:
        return self.role.lower() == "admin"

# ĐÃ SỬA: Thêm user_id vào request body
class ChatRequest(BaseModel):
    user_id: int = Field(description="ID của user hoặc session_id để lưu lịch sử chat")
    message: str

# THIẾT KẾ OUTPUT CHUẨN PRODUCTION
class ChatResponse(BaseModel):
    text: str = Field(description="Nội dung AI chat với người dùng")
    intent: str = Field(default="general", description="Phân loại luồng xử lý (general, product_search, order_tracking...)")
    data: Dict[str, Any] = Field(default_factory=dict, description="Payload dữ liệu động cho Frontend (VD: product_ids)")
    session_id: str = Field(description="ID của phiên chat")

# --- 2. MOCK DATA BINDING ---
# ĐÃ SỬA: Bỏ Depends(get_current_user) fix cứng, chuyển thành hàm tạo user động dựa trên ID
def get_mock_user(user_id: str) -> UserContext:
    # Trong thực tế, bạn sẽ query database ở đây: db.query(User).filter(id == user_id)
    return UserContext(
        user_id=user_id,
        username=f"user_{user_id}",
        email=f"user{user_id}@gmail.com",
        role="buyer", 
        token="mock_token_for_dev_mode"
    )


# --- 3. ENDPOINTS ---
@api.post("/api/ai/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest):
    try:
        # Lấy thông tin user dựa trên user_id gửi lên
        user = get_mock_user(request.user_id)
        
        # SỬA Ở ĐÂY 1: Ép kiểu user.user_id sang chuỗi (str) cho thread_id
        config = {"configurable": {"thread_id": str(user.user_id)}}
        
        initial_state = {
            "user_prompt": request.message,
            "log_action": [],
            "messages": [HumanMessage(content=request.message)],
            "ui_data": {}  # Initialize empty ui_data for consistency
        }
        
        # Invoke the orchestrator graph
        result = await buyer_orchestrator_app.ainvoke(initial_state, config=config)
        
        # Extract data from MasterState with proper defaults
        response_text = result.get("answer", "Done.")
        response_data = result.get("ui_data", {})  # Structured data for frontend
        current_intent = result.get("intent", "general")

        # Return consistent response format
        return ChatResponse(
            text=response_text,
            intent=current_intent,
            data=response_data,
            # SỬA Ở ĐÂY 2: Ép kiểu user.user_id sang chuỗi (str)
            session_id=str(user.user_id) 
        )

    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

# ... (giữ nguyên các phần dưới)

if __name__ == "__main__":
    print("🚀 Dev Server running on http://0.0.0.0:8000")
    uvicorn.run(api, host="0.0.0.0", port=8000)