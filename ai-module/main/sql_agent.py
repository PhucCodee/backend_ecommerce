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
    "gemini-2.5-flash", model_provider="google_genai", temperature=0,
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
Tables:
1. products (product_id, product_name, description, status, moderation_status)
2. product_skus (sku_id, product_id, sku, price, is_default)
3. inventory (sku_id, quantity_available)
4. orders (order_id, user_id, status, total_amount, created_at, order_number)
5. order_items (order_id, sku_id, product_name, quantity, unit_price, subtotal)
6. users (user_id, email, username)

Relationships:
- inventory links to product_skus via sku_id
- product_skus links to products via product_id
- orders link to users via user_id
- order_items link orders and product_skus
"""

# --- Data Models ---
class SQLQueryGenerator(BaseModel):
    """Structure for the LLM's generated SQL."""
    query: str = Field(..., description="The syntactically correct PostgreSQL query to run.")
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
    """Node 1: Translate user natural language to SQL."""
    structured_llm = llm.with_structured_output(SQLQueryGenerator)
    last_message = state["messages"][-1]
    
    system_prompt = f"""You are a PostgreSQL expert for an E-commerce system.
    Your goal is to convert user questions into valid SQL queries based on the schema provided.
    
    {DB_SCHEMA}
    
    Rules:
    1. Only generate SELECT statements. Never generate INSERT, UPDATE, or DELETE.
    2. Use 'ILIKE' for text matching instead of '=' for case-insensitivity.
    3. To get product price, join 'products' with 'product_skus'.
    4. To get stock, join 'product_skus' with 'inventory'.
    5. Always limit results to 5 unless asked otherwise.
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

if __name__ == "__main__":
    run_sql_agent()