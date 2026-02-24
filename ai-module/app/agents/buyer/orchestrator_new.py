#orchestrator_new.py

from email import message
import os
from dotenv import load_dotenv
from typing import Annotated, Literal, List, Dict, Any
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
from IPython.display import Image, display
from pathlib import Path

load_dotenv()

if not os.environ.get("GOOGLE_API_KEY"):
    print("Warning: GOOGLE_API_KEY not found.")

llm = init_chat_model(
    "llama-3.3-70b-versatile", 
    model_provider="groq",
    temperature=0.1,
)

gerneral_llm =  init_chat_model(
    "llama-3.1-8b-instant", 
    model_provider="groq",
    temperature=0.1,
)

class MasterState(TypedDict):
    user_question: str | None
    messages: Annotated[list[BaseMessage], add_messages]
    # Keys for SQL Agent to function
    sql_query: str | None
    sql_result: List[Dict[str, Any]] | str | None
    sql_params: List[Any] | None
    # Key for Router decision
    next_step: str | None


class RouterOutput(BaseModel):
    """Classify user intent."""
    intent: Literal["policy", "database", "general"] = Field(
        ...,
        description="The appropriate expert to handle the query."
    )

class SQLQueryGenerator(BaseModel):
    """Structure for the LLM's generated SQL."""
    query: str = Field(..., description="The syntactically correct PostgreSQL query to run.")
    parameters: List[str] = Field(..., description="A list of values to safely substitute into the query's %s placeholders, in order.")
    explanation: str = Field(..., description="Brief explanation of what the query does.")

def router_node(state: MasterState):
    """Classifies the intent of the last message."""
    messages = state["messages"]
    last_user_msg = messages[-1]
    
    structured_llm = gerneral_llm.with_structured_output(RouterOutput)
    
    system_msg = """You are the Router for an E-commerce AI system.
    Route the query to the correct expert:
    
    - 'database': For queries about specific products, prices, stock levels, orders, or user data. (Requires SQL access).
    - 'policy': For general questions about technical issue, shipping, returns, company info, privacy, or "how to" guides. (Requires Document search).
    - 'general': For greetings, compliments, or off-topic conversation.
    """
    
    response = structured_llm.invoke([
        SystemMessage(content=system_msg + f"context: {state['messages']}"),
        HumanMessage(content = last_user_msg.content)
    ])

    return {
        "next_step": response.intent,
        "user_question": last_user_msg.content
    }




#----------------------------- SQL-AGENT  --------------------------------------
def sql_agent(state):
    print("\n--- 🔀 Routing to Policy SQL Agent ---")
    return {}

DB_HOST = os.getenv("DB_HOST", "localhost") 
DB_NAME = os.getenv("DB_NAME", "database")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASS = os.getenv("DB_PASSWORD", "123")
DB_PORT = os.getenv("DB_PORT", "5432")

DB_SCHEMA = """
Database Domain:
- E-commerce platform supporting product discovery, SKU-based selling, inventory control, user management, and transactional order processing.
- Uses SKU-level pricing and inventory with order-time snapshotting to preserve historical accuracy.

==================================================
TABLE DEFINITIONS
==================================================

1. users
   - user_id (PK):
     System-generated unique identifier for each user.
     Used as the primary reference for authentication, ownership, and order attribution.

   - email:
     User’s email address.
     Must be unique across the system.
     Used for login, password recovery, notifications, and transactional communication.

   - username:
     Public-facing unique identifier.
     Displayed in user interfaces, reviews, and admin panels.
     Immutable after creation (recommended).

   - Purpose:
     Represents all platform actors including buyers, sellers, and administrators.

--------------------------------------------------

2. products
   - product_id (PK):
     Unique identifier for a logical product entity.
     Does not represent a sellable unit by itself.

   - product_name:
     Human-readable product title.
     Used in search, listings, and order snapshots.

   - description:
     Long-form textual content describing the product.
     May include features, specifications, and marketing copy.
     Optimized for semantic search and embeddings.

   - status:
     Operational lifecycle state of the product.
     Typical values: draft, active, inactive, archived, deleted.
     Controls whether the product can be displayed or sold.

   - moderation_status:
     Administrative approval state.
     Typical values: pending, approved, rejected.
     Ensures products meet platform policies before activation.

   - Purpose:
     Represents a conceptual product.
     Pricing, inventory, and sellable variants are handled at SKU level.

--------------------------------------------------

3. product_skus
   - sku_id (PK):
     Unique identifier for each sellable product variant.
     Acts as the atomic commercial unit in the system.

   - product_id (FK → products.product_id):
     Links the SKU to its parent product.
     Enforces product-to-variant ownership.

   - sku:
     Human-readable stock keeping unit code.
     Often encodes variant attributes (size, color, model).
     Must be unique across all SKUs.

   - price: (USD)
     Monetary selling price for one unit of this SKU.
     Represents the authoritative price source.
     Used during checkout and copied into order_items at purchase time.

   - is_default:
     Boolean flag indicating whether this SKU is the default variant.
     Used for product listings and quick-add actions.

   - Purpose:
     Represents the actual sellable item variant.
     All commercial transactions occur at SKU level.

--------------------------------------------------

4. inventory
   - sku_id (PK, FK → product_skus.sku_id):
     Identifies the SKU whose stock is being tracked.
     Enforces one-to-one relationship with product_skus.

   - quantity_available:
     Current on-hand inventory count.
     Decremented when orders are placed.
     Used to prevent overselling and show availability.

   - Purpose:
     Tracks real-time stock levels for each SKU.

--------------------------------------------------

5. orders
   - order_id (PK):
     Internal unique identifier for the order record.
     Used for database joins and internal processing.

   - order_number:
     External, human-readable order reference.
     Displayed to users, used in support and invoices.
     Typically sequential or formatted by date.

   - user_id (FK → users.user_id):
     Identifies the user who placed the order.
     Represents the buyer, not necessarily the seller.

   - status:
     Current state of the order lifecycle.
     Typical values: pending, paid, processing, shipped, completed, cancelled.
     Drives fulfillment and refund logic.

   - total_amount:
     Total monetary value of the order.
     Usually equals the sum of all order_items subtotals.
     Stored for fast retrieval and reporting.

   - created_at:
     Timestamp when the order was created.
     Used for sorting, analytics, and auditing.

   - Purpose:
     Represents a purchase transaction initiated by a user.

--------------------------------------------------

6. order_items
   - order_id (FK → orders.order_id):
     Identifies the parent order.
     Groups multiple purchased items under one transaction.

   - sku_id (FK → product_skus.sku_id):
     References the SKU that was purchased.
     Used for inventory deduction and analytics.

   - product_name:
     Snapshot of the product name at time of purchase.
     Preserved even if the product name changes later.

   - quantity:
     Number of units of this SKU purchased in the order.
     Used to calculate subtotal and update inventory.

   - unit_price:
     Price per unit at the time of purchase.
     Copied from product_skus.price to preserve historical pricing.

   - subtotal:
     Calculated as quantity × unit_price.
     Represents the line-item cost contribution to the order total.

   - Purpose:
     Represents individual line items within an order.
     Stores immutable purchase-time data for historical accuracy.

==================================================
RELATIONSHIPS
==================================================

- users 1 → N orders
  A user may place multiple orders over time.

- products 1 → N product_skus
  Each product may have multiple sellable variants.

- product_skus 1 → 1 inventory
  Every SKU has exactly one inventory record.

- orders 1 → N order_items
  An order consists of one or more line items.

- product_skus 1 → N order_items
  A SKU may appear in many orders across its lifetime.

==================================================
BUSINESS RULES & CONSTRAINTS
==================================================

- Pricing authority resides exclusively in product_skus.
- products must never store price information.
- Inventory quantity must exist for every SKU.
- Inventory is decremented based on order_items.quantity.
- order_items store snapshot data to ensure historical consistency.
- Orders may include multiple SKUs from the same or different products.
"""

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


def generate_query_node(state: MasterState):
    """Node 1: Translate user natural language to SQL with Semantic Expansion."""
    structured_llm = llm.with_structured_output(SQLQueryGenerator)
    last_message = state["messages"][-1]
    
    # 1. We update the prompt to encourage synonym logic
    system_prompt = f"""You are a smart PostgreSQL expert for an E-commerce system.
    Your goal is to convert user questions into valid SQL queries based on the schema provided.
    
    {DB_SCHEMA}
    
    **CRITICAL RULES FOR "SMART" SEARCH:**
    1. **Expand Keywords:** Users often use different words than the database. If a user searches for a concept (e.g., "hazard weather"), you MUST search for synonyms (e.g., 'extreme', 'severe', 'heavy', 'protection').
       - **BAD:** WHERE description ILIKE '%hazard weather%'
       - **GOOD:** WHERE (description ILIKE '%hazard%' OR description ILIKE '%extreme%' OR description ILIKE '%severe%' OR description ILIKE '%heavy%')
    
    2. **Fuzzy Matching:** Use 'ILIKE' for text matching. 
    
    3. **Product Name Priority:** Always prioritize matching the `product_name` first, then `description`.
    
    4. **Join Rules:**
       - To get product price, join 'products' with 'product_skus'.
       - To get stock, join 'product_skus' with 'inventory'.
       
    5. **Limit:** Always limit results to 5 unless asked otherwise.

    6. **HANDLE TYPOS & FUZZY SEARCH:** - If the user searches for a specific product name (e.g., 'USA KeyBoard'), do NOT rely on a single strict 'ILIKE'.

       - Instead, try to match ANY of the key terms or use SIMILARITY sorting.

       - **Preferred Pattern:** SELECT product_name, price, description

         FROM products

         WHERE product_name ILIKE '%KeyBoard%'

         ORDER BY SIMILARITY(product_name, 'USA KeyBoard') DESC

         LIMIT 5;
    """
    
    result = structured_llm.invoke(
        [SystemMessage(content =  system_prompt)] + state["messages"]
    )
    
    return {
        "sql_query": result.query,
        "sql_params": result.parameters
    }


def execute_query_node(state: MasterState):
    """Node 2: Execute the generated SQL against the real database."""
    query = state["sql_query"]
    conn = get_db_connection()
    if not conn:
        return {"sql_result": "Error: Could not connect to database."}
    try:
        # RealDictCursor returns results as a dictionary (JSON-friendly)
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            print(f"[Executing SQL]: {query}") # Log for debugging
            if not query.lower().startswith(("select", "with")):
                return {"sql_result": "Only read-only queries are allowed."}

            cur.execute(query)
            
            if cur.description:
                results = cur.fetchall()
                # Convert RealDictRow to standard dict
                clean_results = [dict(row) for row in results]
            else:
                clean_results = [{"status": "Query executed successfully (no return data)"}]
            
            conn.commit()
            return {"sql_result": clean_results}
            
    except Exception as e:
        return {"sql_result": f"SQL Execution Error: {str(e)}"}
    finally:
        if conn:
            conn.close()

def synthesize_answer_node(state: MasterState):
    """Node 3: Convert SQL results back to natural language."""
    query = state["sql_query"]
    results = state["sql_result"]
    original_question = state["user_question"]

    
    system_prompt = """You are a helpful e-commerce assistant. 
    Given a user question, the SQL query run, and the data results, formulate a natural language response.
    
    - If the result list is empty, politely say you couldn't find that information.
    - Do not expose database IDs or technical SQL terms to the user.
    - Format currency (VND/USD) appropriately based on the data.
    """
    
    user_content = f"""
    Context: {state['messages']}
    Question: {original_question}
    Query Run: {query}
    Data Results: {results}
    """
    
    response = llm.invoke([
        {"role": "system", "content": system_prompt + user_content},
        {"role": "user", "content": original_question}
    ])
    
    return {"messages": [response]}





#----------------------------- END SQL-AGENT  --------------------------------------


#-------------------------      RAG-AGENT      ---------------------------
def rag_agent(state:MasterState):
    print("\n--- 🔀 Routing to Policy (RAG) Agent ---")
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
    print(f"Sau khi đổi")
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
    messages = [SystemMessage(content="""
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
""")] + state["messages"]
    message = tool_llm.invoke(messages)
    return {'messages': [message]}


# Retriever Agent
def take_action(state: MasterState) -> MasterState:
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



#----------------------------- END RAG-AGENT  --------------------------------------
def general_agent(state:MasterState):
    print("\n--- 🔀 Routing to General Chat ---")
    # Simple direct response for greetings

    msg = llm.invoke(
        [SystemMessage(content="You are a helpful e-commerce assistant. Respond politely and concisely.")]+
        state["messages"]
    )

    return {"messages": [AIMessage(content = msg.content)]}


workflow = StateGraph(MasterState)

workflow.add_node("router", router_node)


workflow.add_node("sql_branch", sql_agent)
workflow.add_node("generate_query", generate_query_node)
workflow.add_node("execute_query", execute_query_node)
workflow.add_node("synthesize_answer", synthesize_answer_node)



workflow.add_node("rag_branch", rag_agent)
workflow.add_node("llm", call_llm)
workflow.add_node("retriever_agent", take_action)

workflow.add_node("general_branch", general_agent)

workflow.add_edge(START, "router")


# def route_decision(state: MasterState):
#     return state["next_step"]

workflow.add_conditional_edges(
    "router",
    lambda state: state["next_step"],
    {
        "database": "sql_branch",
        "policy": "rag_branch",
        "general": "general_branch"
    }
)

workflow.add_edge("sql_branch", "generate_query")
workflow.add_edge("generate_query", "execute_query")
workflow.add_edge("execute_query", "synthesize_answer")
workflow.add_edge("synthesize_answer", END)

workflow.add_edge("rag_branch", "llm")
workflow.add_conditional_edges(
    "llm",
    should_continue,
    {True: "retriever_agent", False: END}
)
workflow.add_edge("retriever_agent", "llm")


workflow.add_edge("general_branch", END)

checkpointer = MemorySaver()
app = workflow.compile(checkpointer=checkpointer)


if __name__ == "__main__":
    print("=== ORCHESTRATOR AGENT STARTED ===")
    print("Type 'exit' to quit.\n")

    # 1. Define a thread ID. In a real app, this would be the User ID or Session ID.
    thread_id = "user-session-123"
    config = {"configurable": {"thread_id": thread_id}}

    while True:
        try:
            user_input = input("User: ")
            if user_input.lower() in ["exit", "quit"]:
                break
            
            # 2. Prepare the input
            # We ONLY send the new message. LangGraph automatically pulls 
            # the *old* messages from memory because we provided the thread_id.
            input_payload = {"messages": [HumanMessage(content=user_input)]}
            
            # 3. Invoke with config
            # usage of 'config' is CRITICAL for memory
            result = app.invoke(input_payload, config=config)
            
            # Extract the final response
            final_msg = result["messages"][-1].content
            print(f"Assistant: {final_msg}\n")
            
        except Exception as e:
            print(f"An error occurred: {e}")
            # Optional: Print stack trace for debugging
            import traceback
            traceback.print_exc()
