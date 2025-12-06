import uvicorn
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from langchain_core.messages import HumanMessage

# Import your compiled graph
from orchestrator import app as orchestrator_app

# Initialize FastAPI
api = FastAPI(
    title="E-Commerce AI Chatbot API",
    description="Orchestrator bridging SQL, RAG, and General Chat agents.",
    version="1.0"
)

# --- CORS Configuration ---
# This allows your React Frontend (usually localhost:3000) to talk to this API
origins = [
    "http://localhost:3000",
    "http://localhost:5173", # Vite default
    "http://127.0.0.1:3000",
]

api.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- Data Models ---
class ChatRequest(BaseModel):
    message: str
    thread_id: str  # Unique ID for the user session (e.g., "user-123")

class ChatResponse(BaseModel):
    response: str
    thread_id: str

# --- Endpoints ---

@api.get("/health")
def health_check():
    return {"status": "running", "service": "orchestrator"}

@api.post("/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatRequest):
    try:
        print(f"Incoming request from thread {request.thread_id}: {request.message}")
        
        # 1. Prepare the input for the graph
        initial_state = {"messages": [HumanMessage(content=request.message)]}
        
        # 2. Configure the session ID (Thread)
        # This tells LangGraph to look up previous history for this specific user
        config = {"configurable": {"thread_id": request.thread_id}}
        
        # 3. Run the graph asynchronously
        # We use 'ainvoke' for better performance in FastAPI
        result = await orchestrator_app.ainvoke(initial_state, config=config)
        
        # 4. Extract the AI's last message
        ai_message = result["messages"][-1].content
        
        return ChatResponse(
            response=ai_message,
            thread_id=request.thread_id
        )

    except Exception as e:
        print(f"Error processing request: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# --- Run Server ---
if __name__ == "__main__":
    print("Starting FastAPI Server on port 8000...")
    uvicorn.run(api, host="0.0.0.0", port=8000)