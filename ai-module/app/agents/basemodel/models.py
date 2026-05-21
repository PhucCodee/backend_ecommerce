# Consolidated basemodel - Contains all models, enums, actions, and state definitions
from typing import Annotated, Literal, List, Tuple, Dict, Any
try:
    from typing_extensions import TypedDict
except ImportError:
    from typing import TypedDict
from enum import Enum
from pydantic import BaseModel, Field
from abc import ABC
from dataclasses import dataclass
import operator
from langchain_core.messages import BaseMessage
from langgraph.graph.message import add_messages


# ──────────────────────────────────────────────────────────────────────────────
# 1. PRODUCT MODEL
# ──────────────────────────────────────────────────────────────────────────────

class Product(BaseModel):
    name: str = Field(
        description="The core product noun only (e.g., 'jacket', 'shirt', 'shoes'). No adjectives."
    )
    des: List[str] = Field(
        default_factory=list,
        description="List of isolated descriptive attributes or modifiers (e.g., ['blue', 'size L', 'waterproof'])."
    )


# ──────────────────────────────────────────────────────────────────────────────
# 2. ORDER-RELATED ENUMS & MODELS
# ──────────────────────────────────────────────────────────────────────────────



class Order(BaseModel):
    order_number:   str = Field(
        default="",
        description="Exact order number if mentioned (e.g. ORD-2025-001234), else empty string"
    )
    time_context:   Literal["any", "newest", "oldest", "today", "yesterday", "this_week", "last_week", "this_month"] = Field(
        default="any",
        description="Temporal reference to identify which order"
    )
    order_intent:   Literal["track_status", "track_location", "cancel_order", "request_refund", "confirm_action"] = Field(
        default="track_status",
        description="What the user actually wants to do with the order"
    )

# ──────────────────────────────────────────────────────────────────────────────
# 3. ACTION CLASSES
# ──────────────────────────────────────────────────────────────────────────────

class Action(ABC):
    """Base class for all action types"""
    def __init__(self, intent: Literal["policy_question", "product_search", "general", "order_tracking"], 
                 user_prompt: str = "", answer: str = ""):
        self.intent = intent
        self.user_prompt = user_prompt
        self.answer = answer
        
    def set_user_prompt(self, human_mes):
        self.user_prompt = human_mes
        return self
    
    def set_answer(self, ai_ans):
        self.answer = ai_ans
        return self


@dataclass
class PolicyAction(Action):
    """Action for policy-related questions"""
    def __init__(self, human_mes: str = "", ai_ans: str = ""):
        super().__init__(intent="policy_question", user_prompt=human_mes, answer=ai_ans)
    
    def set_context(self, context):
        self.context = context
        return self

    def get_context(self):
        return self.context


@dataclass
class GeneralAction(Action):
    """Action for general questions"""
    def __init__(self, human_mes: str = "", ai_ans: str = ""):
        super().__init__(intent="general", user_prompt=human_mes, answer=ai_ans)


@dataclass
class ProductSearchAction(Action):
    """Action for product search queries"""
    def __init__(self, human_mes: str = "", ai_ans: str = "", product: Product = None):
        super().__init__(intent="product_search", user_prompt=human_mes, answer=ai_ans)
        self.product = product
        self.product_res = None
    
    def set_query(self, query):
        self.query = query
        return self
    
    def set_product(self, product):
        self.product = product
        return self
    
    def set_product_res(self, productRes):
        self.product_res = productRes
        return self
    
    def get_query(self):
        return self.query
    
    def get_product_res(self): 
        return self.product_res

    def get_product(self): 
        return self.product

@dataclass
class OrderTrackingAction(Action):
    """Action for order tracking queries"""
    def __init__(self, human_mes: str = "", ai_ans: str = "", order: Order = None):
        super().__init__(intent="order_tracking", user_prompt=human_mes, answer=ai_ans)
        self.order = order
        self.order_res = None
    
    def set_query(self, query):
        self.query = query
        return self
    
    def set_order(self, order):
        self.order = order
        return self
    
    def set_order_res(self, orderRes):
        self.order_res = orderRes
        return self
    
    def get_query(self):
        return self.query
    
    def get_order_res(self): 
        return self.order_res

    def get_order(self): 
        return self.order


# ──────────────────────────────────────────────────────────────────────────────
# 3b. UI RESPONSE MODELS
# ──────────────────────────────────────────────────────────────────────────────

class IntentOutput(BaseModel):
    """Output model for intent classification"""
    intent: Literal["policy_question", "product_search", "order_tracking"] = Field(...)


class BaseUIResponse(BaseModel):
    """Base class for all UI responses - ensures consistency"""
    text: str = Field(description="Natural language response from the AI (concise)")


class ProductUIResponse(BaseUIResponse):
    """Product search response"""
    product_ids: List[int] = Field(default_factory=list, description="List of product IDs found")


class OrderUIResponse(BaseUIResponse):
    """Order tracking response"""
    order_ids: List[int] = Field(default_factory=list, description="List of order IDs found")
    order_numbers: List[str] = Field(default_factory=list, description="List of order numbers")


class GeneralUIResponse(BaseUIResponse):
    """General/Policy response with optional source citations"""
    sources: List[Dict[str, str]] = Field(default_factory=list, description="Source citations if applicable")


# ──────────────────────────────────────────────────────────────────────────────
# 4. MASTER STATE
# ──────────────────────────────────────────────────────────────────────────────

class MasterState(TypedDict):
    """Main state container for the chatbot agent"""
    messages: Annotated[list[BaseMessage], add_messages]
    user_prompt: str
    answer: str
    log_action: Annotated[List[Action], operator.add]
    notification: str
    intent: str
    customer_id: str
    # UI data for structured information (MCP, UI)
    ui_data: Dict[str, Any]
    faq_hit: bool
