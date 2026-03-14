#agent.py

from email import message
import enum
from itertools import product
import os
from re import S
from dotenv import load_dotenv
from typing import Annotated, Literal, List, Dict, Any, Tuple
from typing_extensions import TypedDict
import psycopg2
from psycopg2.extras import RealDictCursor
from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from langchain_core.messages import HumanMessage, BaseMessage, AIMessage, SystemMessage,ToolMessage
from langchain.chat_models import init_chat_model
from pydantic import BaseModel, Field
from langgraph.checkpoint.memory import MemorySaver
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain_chroma import Chroma
from langchain_core.tools import tool
from langchain_core.runnables import RunnableConfig
from pathlib import Path
from abc import ABC, abstractmethod
from dataclasses import dataclass

load_dotenv()


llm = init_chat_model(
    "llama-3.3-70b-versatile", 
    model_provider="groq",
    temperature=0.1,
)

class Action(ABC):
    def __init__(self, intent: Literal["policy_question", "product_search", "general", "order_tracking","mixed"],
                 user_prompt: str = "",
                 answer:str = ""):
        self.intent = intent
        self.user_prompt = user_prompt
        self.answer = answer

    def set_user_prompt(self,human_mes):
        self.user_prompt = human_mes
        return self
    
    def set_answer(self,ai_ans):
        self.answer = ai_ans
        return self
    
class MasterState(TypedDict):
    messages: Annotated[list[BaseMessage], add_messages]
    user_prompt: str
    answer: str
    log_action: List[Action]
    intent: str
    customer_id: str

class IntentOutput(BaseModel):
    """Classify user intent."""
    intent: Literal["policy_question", "product_search", "general", "order_tracking","mixed"] = Field(
        ...,
        description="The appropriate expert to handle the query."
    )

@dataclass
class PolicyAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= ""):
        super().__init__(intent = "policy_question", user_prompt = human_mes, answer=ai_ans)


@dataclass
class GeneralAction(Action):
    def __init__(self, human_mes :str = "", ai_ans:str= ""):
        super().__init__(intent = "general", user_prompt = human_mes, answer=ai_ans)

    

def intent_classifier(state: MasterState,config: RunnableConfig) -> MasterState:
    user_id = config["configurable"].get("thread_id")
    prompt = """
    You are an intent classification engine for a production e-commerce AI system.
    Your ONLY task is to classify the user's latest message into ONE of the predefined intent categories.

    ---------------------------------------
    INTENT CATEGORIES
    ---------------------------------------

    1. "product_search"
    - User is looking for products.
    - Examples:
        - "I want black nike shoes under $100"
        - "Show me gaming laptops"
        - "Do you have waterproof jackets?"
        - "Find me a keyboard"
        - "Do you have any shirts"

    2. "order_track"
    - User is asking about a specific order.
    - Includes:
        - Order status
        - Order tracking
        - Delivery updates
        - Order cancellation
        - Refund status
    - Examples:
        - "Where is my order?"
        - "Track order #12345"
        - "Has my package shipped?"
        - "I want to cancel my order"
        - "I just cancle my newest order, is it succesfully canceled"
        - "Is my newest order on shipping"

    3. "policy_question"
    - User is asking about store policies or company rules.
    - Includes:
        - Return policy
        - Shipping policy
        - Refund policy
        - Payment methods
        - Privacy policy
        - Warranty information
    - Examples:
        - "What is your return policy?"
        - "How long does shipping take?"
        - "Do you accept visa card?"
        - "How can I get help with technical issue"
        - "Is my personal information privately protected"
    4. "general"
    - Greetings, small talk, compliments, or unrelated questions.
    - Examples:
        - "Hi"
        - "Thanks"
        - "You're helpful"
        - "What’s your name?"

    5. "mixed"
    - The message clearly contains more than one of the above intents.
    - Example:
        - "Can I return the shoes I bought last week?"
        (Requires order lookup + return policy)

    -.
    """
    message = state['user_prompt']
    intent_llm = llm.with_structured_output(IntentOutput)

    response = intent_llm.invoke([
        SystemMessage(content = prompt),
        HumanMessage(content = message)
    ])

    return {
        "intent":response.intent,
        "customer_id": user_id
    }


def general_agent(state:MasterState):
    print("\n--- 🔀 Routing to General Chat ---")
    # Simple direct response for greetings
    msg = llm.invoke(
        [SystemMessage(content="You are a helpful e-commerce assistant. Respond politely and concisely."),
         HumanMessage(content = state["user_prompt"])
        ]
    )
    action = GeneralAction(human_mes=state["user_prompt"],
                           ai_ans=  msg.content )
    
    state["log_action"] += [action]

    return {"answer": msg.content}


workflow = StateGraph(MasterState)
workflow.add_node("general", general_agent)
workflow.add_node("intent_classifier", intent_classifier)
workflow.add_edge(START, "intent_classifier")
workflow.add_conditional_edges(
    "intent_classifier",
    lambda state: state["intent"],
    {
        "product_search": "product_search",
        "policy_question": "policy_faq",
        "order_track": END,
        "general": "general",
        "mixed": END
    }
)



#-------------------------      RAG-AGENT      ---------------------------
def policy_faq(state:MasterState):
    print("\n--- 🔀 Routing to Policy FAQ Agent ---")
    action = PolicyAction(human_mes=state["user_prompt"])
    state["log_action"] += [action]
    state["messages"] = []
    return {}

embeddings = GoogleGenerativeAIEmbeddings(
    model="models/gemini-embedding-001",
)
default_path = Path(__file__).resolve().parents[3] / "data" / "vector"
persist_directory = os.getenv("CHROMA_DB_PATH", str(default_path))


vectorstore = Chroma(
    persist_directory=str(persist_directory), # Chuyển Path thành string cho chắc chắn
    collection_name="rag_document",
    embedding_function=embeddings  
)

retriever = vectorstore.as_retriever(
    search_type="similarity",
    search_kwargs={"k": 5} 
)

@tool
def retriever_tool(query: str) -> str:
    """
    Use this tool to find information about "My Shop" e-commerce store policies.
    This tool is the ONLY source for information on:
    - Return policy, cancellations, and refunds
    - Shipping and delivery information
    - Payment methods
    - Privacy notice and Conditions of Use
    - How to contact customer support
    - Technical issues or device support
    - Introduction to the store
    """
    # print(f"Path: {persist_directory}")
    print(f"Calling retriever_tool with query: {query}")
    docs = retriever.invoke(query) 

    if not docs:
        return "No relevant information found in the store policy documents."
    
    results = []
    for doc in docs:
        source = doc.metadata.get('source_document', 'Unknown')
        page = doc.metadata.get('page', 'N/A')
        results.append(f"Source: {source} (Page {page})\nContent: {doc.page_content}")
    
    return "\n\n".join(results)

tools = [retriever_tool]

tool_llm = llm.bind_tools(tools)
tools_dict = {our_tool.name: our_tool for our_tool in tools} 

# LLM Agent
def call_llm(state: MasterState) -> MasterState:
    """Function to call the LLM with the current state."""
    prompt = """
You are a helpful AI assistant for "My Shop", an e-commerce platform.
Your primary role is to answer customer questions about the store's policies and features.

**Core Rules:**
1. For **all** questions related to store policies, you **MUST** use the `retriever_tool`.
2. **STOP SEARCHING IMMEDIATELY** once you have found relevant information in the tool output. Do not re-query with different keywords if you already have the answer.
3. Answer **ONLY** based on the information provided by the tool.
4. If the tool returns "No relevant information found...", clearly state that and **STOP**.
5. **Citation is mandatory.**

**Citation Example:**
"You can cancel an order as long as it has not yet been shipped (Source: Policy Doc, Page 2)."
"""
    response = tool_llm.invoke([
        SystemMessage(content = prompt), 
    ] + state['messages'])

    return {'messages': response}


# Retriever Agent
def find_context(state: MasterState) -> MasterState:
    """Execute tool calls from the LLM's response."""
    tool_calls = state['messages'][-1].tool_calls
    results = []
    for t in tool_calls:
        if t["name"] not in tools_dict:
            result = "Invalid tool name."
        else:
            result = tools_dict[t["name"]].invoke(
                t["args"].get("query", "")
            )

        results.append(
            ToolMessage(
                tool_call_id=t["id"],
                name=t["name"],
                content=str(result)
            )
        )

    print("Tools Execution Complete. Back to the model!")
    return {'messages': results}


def should_continue(state: MasterState):
    """Check if the last message contains tool calls."""
    result = state['messages'][-1]
    return hasattr(result, 'tool_calls') and len(result.tool_calls) > 0

def faq_synthesize(state:MasterState):
    ai_answer = state['messages'][-1]
    state["log_action"][-1].set_answer(ai_answer.content)

    return {
        'answer': ai_answer.content
    }

workflow.add_node("policy_faq", policy_faq)
workflow.add_node("call_llm", call_llm)
workflow.add_node("find_context", find_context)
workflow.add_node("faq_synthesize", faq_synthesize)

workflow.add_edge("policy_faq","call_llm")
workflow.add_conditional_edges(
    "call_llm",
    should_continue,
    {True: "find_context", False: "faq_synthesize"}
)

workflow.add_edge("find_context","call_llm")
workflow.add_edge("faq_synthesize",END)
#----------------------------- END RAG-AGENT  --------------------------------------


#----------------------------- START PRODUCT SEARCH --------------------------------------
def get_db_connection():
    """Establishes a connection to the PostgreSQL database."""
    try:
        conn = psycopg2.connect(
            host=DB_HOST,
            database=DB_NAME,
            user=DB_USER,
            password=DB_PASS,
            port=DB_PORT
        )
        return conn
    except Exception as e:
        print(f"Database Connection Failed: {e}")
        return None
DB_HOST = os.getenv("DB_HOST", "localhost") 
DB_NAME = os.getenv("DB_NAME", "database")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASS = os.getenv("DB_PASSWORD", "123")
DB_PORT = os.getenv("DB_PORT", "5432")


class Product(BaseModel):
    """A look up product information before searching in database"""
    name:str = Field(..., description = "Name of the product")
    des:str= Field(..., description = "Description of the product, may have some adjective words")
    price: Tuple[Literal["less","equal","greater","unknown","ask"],int] = Field(..., description = """The price of the product ("Less",100), ("Unknown",0), ("ask",0)""")

class ProductRes(BaseModel):
    """Product results after execute sql query"""
    name:str
    price: int
    des:str
    status: int

# class SQLQueryGenerator(BaseModel):
#     """Structure for the LLM's generated SQL."""
#     query: str = Field(..., description="The syntactically correct PostgreSQL query to run.")
#     parameters: List[str] = Field(..., description="A list of values to safely substitute into the query's %s placeholders, in order.")
#     explanation: str = Field(..., description="Brief explanation of what the query does.")

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
    




def product_search(state:MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Product search Agent ---")

    product_llm =  llm.with_structured_output(Product)
    prompt = """
    You are an AI assistance of ecommerce platform. You are beeing asked about some product search to recommend for customer. 
    For ease of search, you have to define some property base on customer latest query

    Example
    User: "Do you have any red shirts"
    -> name: shirt, price: (unknown,0), des: red

    User: "I want to buy a jacket which is waterproof and less than 50 usd"
    -> name: jacket, price: (less,50), des: waterproof

    User: "I want to buy a jacket which is waterproof and less than 50 usd"
    -> name: jacket, price: (less,50), des: waterproof

    User: "How much is the Minimal Logo Tee"
    -> name: Minimal Logo Tee, price: (ask,0), des: Minimal Logo

    You may look at the previous query to get the context

    Example
    User: "Do you have any red shirts"
    AI: <some recommendations given>
    User: "But i want them in blue and cheaper"
    -> name: shirt, price: (less,<price of last recommendation>), des: blue

    """
    response = product_llm.invoke([
        SystemMessage(content = prompt),
        HumanMessage(state['user_prompt']),
    ]
    )
    action = ProductSearchAction().set_product(response)
    state["log_action"] += [action]
    print(f"Response: {response}")
    return {}

def generate_product_query(state: MasterState) ->MasterState:
    """Node 1: Translate user natural language to SQL with Semantic Expansion."""
    # query_llm = llm.with_structured_output(SQLQueryGenerator)
    action = state["log_action"][-1]
    product_model = None
    if isinstance(action, ProductSearchAction):
        product_model = action.get_product()

    unknown_price = product_model.price[0] == "unknown"
    ask_price = product_model.price[0] == "ask"

    convert_prompt = f"""
    Write a sql query to search for product:{product_model.name} 
    """
    price_prompt =  "" if unknown_price else (f"I want to know its price" if ask_price else f"\nwhich has price {product_model.price[0]} {product_model.price[1]} dollar")
    convert_prompt += price_prompt

    description_prompt = "\nThe description column must have word {product_model.des} or something semantically similar"
    convert_prompt += description_prompt
    print(f"Sql promtp : {convert_prompt}")
    # 1. We update the prompt to encourage synonym logic
    system_prompt = f"""You are a smart PostgreSQL expert .
    Your goal is generate correct and sufficient SQL query based on user question.

    Example:
    Human: "Write a sql query to seacrh for product: jacket
            Which has price equal 100 dollars
            The description must have word waterproof or something semantically similar
    --> 
    AI: SELECT p.product_name, ps.price, p.description, p.status
    FROM products p JOIN product_skus ps 
        ON p.product_id = ps.product_id JOIN inventory i 
        ON ps.sku_id = i.sku_id 
    WHERE (p.product_name ILIKE '%waterproof%' OR p.product_name ILIKE '%jacket%') 
        AND ps.price = 100
        OR (p.description ILIKE '%waterproof%' 
        OR p.description ILIKE '%jacket%' 
        OR p.description ILIKE '%rain%' 
        OR p.description ILIKE '%windbreaker%') 
    ORDER BY SIMILARITY(p.product_name, 'waterproof jacket') 
    DESC LIMIT 5 

    Human: "Write a sql query to seacrh for product: Minimal Logo Tee
            I want to know its price
            The description must have word Minimal Logo Tee or something semantically similar
    --> 
    AI: SELECT p.product_name, ps.price, p.description, p.status
    FROM products p JOIN product_skus ps 
        ON p.product_id = ps.product_id JOIN inventory i 
        ON ps.sku_id = i.sku_id 
    WHERE (p.product_name ILIKE '%Minimal Logo Tee%' OR p.product_name ILIKE '%Tee%') 
        OR p.description ILIKE '%Simple Logo%' 
        OR p.description ILIKE '%Basic Tee%' 
        OR p.description ILIKE '%Classic Logo%')
    ORDER BY SIMILARITY(p.product_name, 'Minimal Logo Tee') 
    DESC LIMIT 5 

    CRITICAL NOTE: 
    - For p.description condition always use OR
    - If user want to retrieve the products price, skip the p.price  condition
    - Just only return a full sql query which user can copy paste and plug in to sql (PLAIN SQL TEXT, no \n character)
    """
    
    result = llm.invoke([
        SystemMessage(content =  system_prompt),
        HumanMessage(content = convert_prompt)
    ]
    )
    if isinstance(action, ProductSearchAction):
        action.set_query(result.content)

    print(f" Query: {result.content}")
    return {}

def execute_product_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    query = ""
    if isinstance(action, ProductSearchAction):
        query = action.get_query()
    conn = get_db_connection()

    print(f"Executing query: \n{query}\n")
    if not conn:
        return {}
    
    try:
        # RealDictCursor returns results as a dictionary (JSON-friendly)
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            print(f"[Executing SQL]: {query}") # Log for debugging
            if not query.lower().startswith(("select", "with")):
                raise ValueError("Only SELECT queries allowed")
            cur.execute(query)
            
            if cur.description:
                results = cur.fetchall()
                # Convert RealDictRow to standard dict
                clean_results = [dict(row) for row in results]
            else:
                clean_results = [{"status": "Query executed successfully (no return data)"}]
            if isinstance(action,ProductSearchAction):    
                action.set_product_res(clean_results)
            print(f"Data resutls : {clean_results}")
            conn.commit()
            return {}
            
    except Exception as e:
        return {}
    finally:
        if conn:
            conn.close()
    
def synthesize_product_answer(state: MasterState):
    """Node 3: Convert SQL results back to natural language."""
    action = state["log_action"][-1]

    query = action.get_query()
    results = action.get_product_res()
    original_question = state["user_prompt"]

    
    system_prompt = """You are a helpful e-commerce assistant. 
    Given a user question, the SQL query run, and the data results, formulate a natural language response.
    
    - If the result list is empty, politely say you couldn't find that information.
    - Do not expose database IDs or technical SQL terms to the user.
    - Format currency (USD) appropriately based on the data.

    Don't answer like I just found a good product match your query. Its sound like a robot.
    Instead you should answer like a true human seller: We have this product, bla bla bla.... or We currently not having products fit your findings.

    Your answer should be in this format :
    "We have few options for ...<user query>:
    _ Product 1: information
    _ Product 2: information
    ...
    _ Product n: information
    """
    
    user_content = f"""
    This is the context for answering:
        Query Run: {query}
        Data Results: {results}
    """
    
    response = llm.invoke([
        {"role": "system", "content": system_prompt + user_content},
        {"role": "user", "content": original_question}
    ])
    
    return {"answer": response.content}

workflow.add_node("product_search", product_search)
workflow.add_node("generate_product_query", generate_product_query)
workflow.add_node("execute_product_query", execute_product_query)
workflow.add_node("synthesize_product_answer", synthesize_product_answer)

workflow.add_edge("product_search","generate_product_query")
workflow.add_edge("generate_product_query","execute_product_query")
workflow.add_edge("execute_product_query","synthesize_product_answer")
workflow.add_edge("synthesize_product_answer",END)

#-----------------------------------------------------END PRODUCT SEARCH AGENT--------------------------------------

#----------------------------- START ORDER TRACKING--------------------------------------


class Order(BaseModel):
    """A look up order information before searching in database"""
    des:str = Field(..., description = "Brief description of the order")
    status:  Literal["done", "shipping", "cancel"]  = Field(..., description = """The status of the order""")

@dataclass
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
    


def order_tracking(state:MasterState) -> MasterState:
    print("\n--- 🔀 Routing to Product search Agent ---")

    product_llm =  llm.with_structured_output(Order)
    prompt = """
    You are an AI assistance of ecommerce platform. You are beeing asked about some order to track order information for customer. 
    For ease of search, you have to define some property base on customer latest query

    Example
    User: "Is my newest order shipping"
    -> des: newest, status: shipping

    User: "Is my order on yesterday succesfully set"
    -> des: yesterday , status: shipping

    User: "I just cancle my newest order, is it succesfully canceled"
    -> des: newest , status: cancel
    """
    response = product_llm.invoke([
        SystemMessage(content = prompt),
        HumanMessage(state['user_prompt']),
    ]
    )
    action = OrderTrackingAction().set_order(response)
    state["log_action"] += [action]
    print(f"Response: {response}")
    return {}

def generate_order_query(state: MasterState) ->MasterState:
    """Node 1: Translate user natural language to SQL with Semantic Expansion."""
    # query_llm = llm.with_structured_output(SQLQueryGenerator)
    action = state["log_action"][-1]
    order_model = None
    if isinstance(action, OrderTrackingAction):
        order_model = action.get_order()


    convert_prompt = f"""
    Write a sql query to search for {order_model.des} order 
    Which has status: {order_model.status}
    Bought by user whose id is {state['customer_id']}
    """
   

    # 1. We update the prompt to encourage synonym logic
    system_prompt = f"""You are a smart PostgreSQL expert .
    Your goal is generate correct and sufficient SQL query based on user question.

    Example:
    Human: "Write a sql query to search for newest order 
    Which has status: Shipping
    Bought by user whose id is 1
    -> AI:
    SELECT         
    """
    
    result = llm.invoke([
        SystemMessage(content =  system_prompt),
        HumanMessage(content = convert_prompt)
    ]
    )
    if isinstance(action, OrderTrackingAction):
        action.set_query(result.content)

    print(f" Query: {result.content}")
    return {}

def execute_order_query(state: MasterState) -> MasterState:
    action = state["log_action"][-1]
    query = ""
    if isinstance(action, OrderTrackingAction):
        query = action.get_query()
    conn = get_db_connection()

    print(f"Executing query: \n{query}\n")
    if not conn:
        return {}
    
    try:
        # RealDictCursor returns results as a dictionary (JSON-friendly)
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            print(f"[Executing SQL]: {query}") # Log for debugging
            if not query.lower().startswith(("select", "with")):
                return {}

            cur.execute(query)
            
            if cur.description:
                results = cur.fetchall()
                # Convert RealDictRow to standard dict
                clean_results = [dict(row) for row in results]
            else:
                clean_results = [{"status": "Query executed successfully (no return data)"}]
            if isinstance(action,OrderTrackingAction):    
                action.set_order_res(clean_results)
            print(f"Data resutls : {clean_results}")
            conn.commit()
            return {}
            
    except Exception as e:
        return {}
    finally:
        if conn:
            conn.close()
    
def synthesize_order_answer(state: MasterState):
    """Node 3: Convert SQL results back to natural language."""
    action = state["log_action"][-1]

    query = action.get_query()
    results = action.get_order_res()
    original_question = state["user_prompt"]

    
    system_prompt = """You are a helpful e-commerce assistant. 
    Given a user question, the SQL query run, and the data results, formulate a natural language response.
    

    """
    
    user_content = f"""
    This is the context for answering:
        Query Run: {query}
        Data Results: {results}
    """
    
    response = llm.invoke([
        {"role": "system", "content": system_prompt + user_content},
        {"role": "user", "content": original_question}
    ])
    
    return {"answer": response.content}

workflow.add_node("order_tracking", order_tracking)
workflow.add_node("generate_order_query", generate_order_query)
workflow.add_node("execute_order_query", execute_order_query)
workflow.add_node("synthesize_order_answer", synthesize_order_answer)

workflow.add_edge("order_tracking","generate_order_query")
workflow.add_edge("generate_order_query","execute_order_query")
workflow.add_edge("execute_order_query","synthesize_order_answer")
workflow.add_edge("synthesize_order_answer",END)


#----------------------------- END ORDER TRACKING ---------------------------------------


checkpointer = MemorySaver()
app = workflow.compile(checkpointer=checkpointer)

# if __name__ == "__main__":
#     print("===  AGENT STARTED ===")
#     print("Type 'exit' to quit.\n")

#     # 1. Define a thread ID. In a real app, this would be the User ID or Session ID.
#     thread_id = "user-session-123"
#     config = {"configurable": {"thread_id": thread_id}}

#     while True:
#         try:
#             user_input = input("User: ")
#             if user_input.lower() in ["exit", "quit"]:
#                 break
            
#             # 2. Prepare the input
#             # We ONLY send the new message. LangGraph automatically pulls 
#             # the *old* messages from memory because we provided the thread_id.
#             input_payload = {"user_prompt": user_input,
#                              "log_action":[],
#                              "messages": [HumanMessage(content=user_input)]
#                             }
            
#             # 3. Invoke with config
#             # usage of 'config' is CRITICAL for memory
#             result = app.invoke(input_payload, config=config)
            
#             # Extract the final response
#             final_msg = result["answer"]
#             print(f"Assistant: {final_msg}\n")
            
#         except Exception as e:
#             print(f"An error occurred: {e}")
#             # Optional: Print stack trace for debugging
#             import traceback
#             traceback.print_exc()