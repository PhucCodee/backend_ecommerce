from typing import Annotated, List, Dict, Any, Literal
from typing_extensions import TypedDict
from langgraph.graph.message import add_messages

# The Master State acts as the "Bus" transporting data
class MasterState(TypedDict):
    messages: Annotated[list, add_messages]  # Chat history
    
    # Context specific to SQL Agent
    sql_query: str | None
    sql_result: List[Dict[str, Any]] | str | None
    
    # Context specific to Router
    next_step: Literal["policy", "database", "general"] | None