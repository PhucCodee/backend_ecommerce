# app/agents/state.py
from typing import Annotated, Literal, List, Tuple, Dict, Any
from typing_extensions import TypedDict
from langchain_core.messages import BaseMessage
from langgraph.graph.message import add_messages


import operator

# --- 2. MASTER STATE ---
class MasterState(TypedDict):
    messages: Annotated[list[BaseMessage], add_messages]
    user_prompt: str
    answer: str
    log_action: Annotated[List[Action], operator.add]
    notification: str
    intent: str
    customer_id: str
    
    # 🌟 THÊM TRƯỜNG NÀY: Dành riêng cho dữ liệu cấu trúc (MCP, UI)
    ui_data: Dict[str, Any]
