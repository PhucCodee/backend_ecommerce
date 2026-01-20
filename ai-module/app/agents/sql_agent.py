#sql_agent.py

import os
import psycopg2
from psycopg2.extras import RealDictCursor
from dotenv import load_dotenv
from typing import Annotated, List, Any, Dict
from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from langchain.chat_models import init_chat_model
from pydantic import BaseModel, Field
from typing_extensions import TypedDict

# Load environment variables
load_dotenv()

# --- Configuration ---
if not os.environ.get("GOOGLE_API_KEY"):
    print("Warning: GOOGLE_API_KEY not found in environment variables.")

llm = init_chat_model(
    "llama-3.3-70b-versatile",  model_provider="groq", temperature=0,
)

# --- Database Connection Config ---
# Defaults are set for Local Development (localhost). 
# When running in Docker, the .env file or docker-compose env vars should override these.
DB_HOST = os.getenv("DB_HOST", "localhost") 
DB_NAME = os.getenv("DB_NAME", "database")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASS = os.getenv("DB_PASSWORD", "123")
DB_PORT = os.getenv("DB_PORT", "5432")

# --- Database Schema Context ---
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



# --- Data Models ---
class SQLQueryGenerator(BaseModel):
    """Structure for the LLM's generated SQL."""
    query: str = Field(..., description="The syntactically correct PostgreSQL query to run.")
    parameters: List[str] = Field(..., description="A list of values to safely substitute into the query's %s placeholders, in order.")
    explanation: str = Field(..., description="Brief explanation of what the query does.")

class SqlState(TypedDict):
    """State specifically for the SQL workflow."""
    messages: Annotated[list, add_messages]
    sql_query: str | None
    sql_result: List[Dict[str, Any]] | str | None
    final_answer: str | None

# --- Helper Functions ---
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

# --- Nodes ---

def generate_query_node(state: SqlState):
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
    
    result = structured_llm.invoke([
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": last_message.content}
    ])
    
    return {"sql_query": result.query}

def execute_query_node(state: SqlState):
    """Node 2: Execute the generated SQL against the real database."""
    query = state["sql_query"]
    conn = get_db_connection()
    
    if not conn:
        return {"sql_result": "Error: Could not connect to database."}

    try:
        # RealDictCursor returns results as a dictionary (JSON-friendly)
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            print(f"[Executing SQL]: {query}") # Log for debugging
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

def synthesize_answer_node(state: SqlState):
    """Node 3: Convert SQL results back to natural language."""
    query = state["sql_query"]
    results = state["sql_result"]
    original_question = state["messages"][-1].content
    
    system_prompt = """You are a helpful e-commerce assistant. 
    Given a user question, the SQL query run, and the data results, formulate a natural language response.
    
    - If the result list is empty, politely say you couldn't find that information.
    - Do not expose database IDs or technical SQL terms to the user.
    - Format currency (VND/USD) appropriately based on the data.
    """
    
    user_content = f"""
    Question: {original_question}
    Query Run: {query}
    Data Results: {results}
    """
    
    response = llm.invoke([
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": user_content}
    ])
    
    return {"messages": [response]}

# --- Graph Construction ---
sql_graph_builder = StateGraph(SqlState)

sql_graph_builder.add_node("generate_query", generate_query_node)
sql_graph_builder.add_node("execute_query", execute_query_node)
sql_graph_builder.add_node("synthesize_answer", synthesize_answer_node)

sql_graph_builder.add_edge(START, "generate_query")
sql_graph_builder.add_edge("generate_query", "execute_query")
sql_graph_builder.add_edge("execute_query", "synthesize_answer")
sql_graph_builder.add_edge("synthesize_answer", END)

sql_graph = sql_graph_builder.compile()

# --- Runner ---
def run_sql_agent():
    print("SQL Agent ready (Connected to Real DB). Type 'exit' to quit.")
    print("Note: Ensure your Docker DB is running or you are connecting to localhost.")
    
    while True:
        user_input = input("You: ")
        if user_input.lower() == "exit":
            break
            
        input_payload = {"messages": [{"role": "user", "content": user_input}]}
        
        final_state = None
        try:
            for event in sql_graph.stream(input_payload, stream_mode="values"):
                final_state = event
            
            if final_state and final_state["messages"]:
                print(f"Agent: {final_state['messages'][-1].content}")
        except Exception as e:
            print(f"Error running graph: {e}")

# At the bottom of sql_agent.py
if __name__ == "__main__":
    run_sql_agent()