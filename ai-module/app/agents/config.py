# app/agents/config.py
import os
import psycopg2
from pathlib import Path
from dotenv import load_dotenv
from langchain.chat_models import init_chat_model



load_dotenv()

# --- 1. LLM ---
# llm = init_chat_model("llama-3.3-70b-versatile", model_provider="groq", temperature=0.1)
llm = init_chat_model("llama-3.1-8b-instant", model_provider="groq", temperature=0.1)

# --- 2. DATABASE ---
DB_HOST = os.getenv("DB_HOST", "") 
DB_NAME = os.getenv("DB_NAME", "database")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASS = os.getenv("DB_PASSWORD", "123")
DB_PORT = os.getenv("DB_PORT", "5432")

def get_db_connection():
    try:
        return psycopg2.connect(host=DB_HOST, database=DB_NAME, user=DB_USER, password=DB_PASS, port=DB_PORT)
    except Exception as e:
        print(f"Database Connection Failed: {e}")
        print(DB_HOST)
        print(DB_NAME)
        print(DB_USER)
        print(DB_PASS)
        print(DB_PORT)
        return None
