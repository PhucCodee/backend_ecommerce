import uvicorn
import uuid
from typing import Optional
from fastapi import FastAPI, HTTPException, Depends
from pydantic import BaseModel, EmailStr
from langchain_core.messages import HumanMessage

# Import your graph
from app.agents.buyer.orchestrator_new import app as buyer_orchestrator_app
from app.agents.admin.orchestrator import app as admin
from app.agents.seller.orchestrator import app as seller_orchestrator_app


api = FastAPI(title="AI Microservice - Dev Mode")

# --- 1. DATA MODELS ---

class UserContext(BaseModel):
    user_id: str
    username: str
    email: EmailStr
    role: str
    token: Optional[str] = None 
    
    @property
    def is_admin(self) -> bool:
        return self.role.lower() == "admin"

class ChatRequest(BaseModel):
    message: str
    session_id: str

class ChatResponse(BaseModel):
    response: str
    session_id: str

# --- 2. MOCK AUTH DEPENDENCY ---
# We removed HTTPBearer. This function now runs automatically
# without checking for Authorization headers.

async def get_current_user() -> UserContext:
    """
    DEV MODE: Always returns the hardcoded 'Ronaldo' user.
    No security libraries (jwt/HTTPBearer) are involved.
    """
    return UserContext(
        user_id="1",
        username="goat",
        email="ronaldo@gmail.com",
        role="buyer", 
        token="mock_token_for_dev_mode"
    )

# --- 3. ENDPOINTS ---

@api.post("/api/ai/start-session")
async def start_session(user: UserContext = Depends(get_current_user)):
    """
    Simulates starting a session.
    """
    session_id = str(uuid.uuid4())
    
    return {
        "session_id": session_id, 
        "message": f"Hello {user.username} (Role: {user.role}), how can I help you?",
        "role_detected": user.role
    }

@api.post("/api/ai/chat", response_model=ChatResponse)
async def chat_endpoint(
    request: ChatRequest, 
    user: UserContext = Depends(get_current_user)
):
    try:
        # 1. Config for thread persistence
        config = {"configurable": {"thread_id": request.session_id}}
        
        # 2. Inject Pydantic model as a dict into LangGraph state
        initial_state = {
            "messages": [HumanMessage(content=request.message)],
            "user_context": user.model_dump() 
        }
        
        # 3. Invoke Agent
        result = await buyer_orchestrator_app.ainvoke(initial_state, config=config)
        
        # 4. Get response
        ai_message = result["messages"][-1].content
        
        return ChatResponse(response=ai_message, session_id=request.session_id)

    except Exception as e:
        print(f"❌ Error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    # Ensure you have 'email-validator' installed for EmailStr:
    # pip install email-validator
    print("🚀 Dev Server running on http://0.0.0.0:8000")
    uvicorn.run(api, host="0.0.0.0", port=8000)