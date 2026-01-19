from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from app.agents.orchestrator import orchestrator_app
from langchain_core.messages import HumanMessage

app = FastAPI(title="Ecommerce AI Service")

class ChatRequest(BaseModel):
    user_id: str
    message: str

@app.post("/api/v1/chat")
async def chat(payload: ChatRequest):
    try:
        # Config provides memory persistence per user_id
        config = {"configurable": {"thread_id": payload.user_id}}
        
        # Run the graph
        input_data = {"messages": [HumanMessage(content=payload.message)]}
        result = await orchestrator_app.ainvoke(input_data, config=config)
        
        # Return the final AI response
        return {"response": result["messages"][-1].content}
        
    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail="AI processing failed")