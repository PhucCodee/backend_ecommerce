import psycopg2
from psycopg2.extras import RealDictCursor
from app.core.config import settings

def get_db_connection():
    try:
        return psycopg2.connect(
            host=settings.DB_HOST,
            database=settings.DB_NAME,
            user=settings.DB_USER,
            password=settings.DB_PASS,
            port=settings.DB_PORT
        )
    except Exception as e:
        print(f"❌ DB Connection Error: {e}")
        return None

def execute_sql_tool(query: str):
    """Executes a SQL query and returns list of dicts."""
    conn = get_db_connection()
    if not conn:
        return "Error: Database unavailable."
    
    try:
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            cur.execute(query)
            if cur.description:
                return [dict(row) for row in cur.fetchall()]
            conn.commit()
            return "Action successful."
    except Exception as e:
        return f"SQL Error: {str(e)}"
    finally:
        conn.close()