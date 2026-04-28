# app/agents/state.py
from typing import Annotated, Literal, List, Tuple, Dict, Any
from typing_extensions import TypedDict
from pydantic import BaseModel, Field
from abc import ABC

# --- 3. PYDANTIC MODELS ---
class IntentOutput(BaseModel):
    intent: Literal["policy_question", "product_search", "general", "order_tracking"] = Field(...)

# Standardized UI Response Models for Consistency Across All Intents
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

