# app/agents/state.py
from typing import  Literal
from typing_extensions import TypedDict

from abc import ABC
from dataclasses import dataclass
import operator


# --- 1. ACTIONS ---
class Action(ABC):
    def __init__(self, intent: Literal["policy_question", "product_search", "general", "order_tracking"], user_prompt: str = "", answer:str = ""):
        self.intent = intent
        self.user_prompt = user_prompt
        self.answer = answer
        
    def set_user_prompt(self, human_mes): self.user_prompt = human_mes; return self
    def set_answer(self, ai_ans): self.answer = ai_ans; return self

@dataclass
class PolicyAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= ""):
        super().__init__(intent = "policy_question", user_prompt = human_mes, answer=ai_ans)
    
    def set_context(self,context):
        self.context = context

    def get_context(self,context):
        self.context = context

@dataclass
class GeneralAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= ""):
        super().__init__(intent = "general", user_prompt = human_mes, answer=ai_ans)

    
@dataclass
class ProductSearchAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= "", product: Product = None):
        super().__init__(intent = "product_search", user_prompt = human_mes, answer=ai_ans)
        self.product = product
        self.product_res = None
    
    def set_query(self,query):
        self.query = query
        return self
    
    def set_product(self,product):
        self.product = product
        return self
    
    def set_product_res(self,productRes):
        self.product_res = productRes
        return self
    
    def get_query(self):
        return self.query
    
    def get_product_res(self): 
        return self.product_res

    def get_product(self): 
        return self.product    


class OrderTrackingAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= "", order: Order = None):
        super().__init__(intent = "order_tracking", user_prompt = human_mes, answer=ai_ans)
        self.order = order
        self.order_res = None
    
    def set_query(self,query):
        self.query = query
        return self
    
    def set_order(self,order):
        self.order = order
        return self
    
    def set_order_res(self,orderRes):
        self.order_res = orderRes
        return self
    
    def get_query(self):
        return self.query
    
    def get_order_res(self): 
        return self.order_res

    def get_order(self): 
        return self.order
    


