# app/agents/state.py
from typing import  Literal,Tuple

from pydantic import BaseModel, Field
from abc import ABC
from dataclasses import dataclass
import operator



class Product(BaseModel):
    name: str = Field(...)
    des: str= Field(...)
    price: Tuple[Literal["less","equal","greater","unknown","ask"],int] = Field(...)





# ── Model ────────────────────────────────────────────────────────────────────

class OrderStatus(str, Enum):
    any         = "any"
    pending     = "pending"       # 0 - chờ xác nhận
    confirmed   = "confirmed"     # 1 - đã xác nhận  
    processing  = "processing"    # 2 - đang xử lý
    shipped     = "shipped"       # 3 - đang giao
    delivered   = "delivered"     # 4 - đã giao
    cancelled   = "cancelled"     # 5 - đã hủy
    refunded    = "refunded"      # 6 - đã hoàn tiền

class OrderTimeContext(str, Enum):
    any         = "any"
    newest      = "newest"
    oldest      = "oldest"
    today       = "today"
    yesterday   = "yesterday"
    this_week   = "this_week"
    last_week   = "last_week"
    this_month  = "this_month"

class OrderIntent(str, Enum):
    track_status    = "track_status"     # hỏi trạng thái đơn hàng
    track_location  = "track_location"   # hỏi vị trí/tracking number
    cancel_order    = "cancel_order"     # yêu cầu/xác nhận hủy đơn
    request_refund  = "request_refund"   # yêu cầu/hỏi trạng thái hoàn tiền
    confirm_action  = "confirm_action"   # xác nhận hành động vừa làm

class Order(BaseModel):
    order_number:   str             = Field(default="",                 description="Exact order number if mentioned (e.g. ORD-2025-001234), else empty string")
    time_context:   OrderTimeContext= Field(default=OrderTimeContext.any,description="Temporal reference to identify which order")
    status_filter:  OrderStatus     = Field(default=OrderStatus.any,    description="Order status the user is asking about or referencing")
    order_intent:   OrderIntent     = Field(default=OrderIntent.track_status, description="What the user actually wants to do with the order")
    needs_tracking: bool            = Field(default=False,              description="True if user wants shipping/delivery location info")
    needs_refund:   bool            = Field(default=False,              description="True if user is asking about or requesting a refund")


