# app/agents/config.py
import os
import psycopg2
from pathlib import Path
from dotenv import load_dotenv
from langchain.chat_models import init_chat_model
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain_chroma import Chroma

load_dotenv()

# --- 1. LLM ---
llm = init_chat_model("llama-3.1-8b-instant", model_provider="groq", temperature=0.1)

# --- 2. DATABASE ---
DB_HOST = os.getenv("DB_HOST", "localhost") 
DB_NAME = os.getenv("DB_NAME", "database")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASS = os.getenv("DB_PASSWORD", "123")
DB_PORT = os.getenv("DB_PORT", "5432")

def get_db_connection():
    try:
        return psycopg2.connect(host=DB_HOST, database=DB_NAME, user=DB_USER, password=DB_PASS, port=DB_PORT)
    except Exception as e:
        print(f"Database Connection Failed: {e}")
        return None

# --- 3. CHROMA DB ---
embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")

# Lưu ý: Vì config.py nằm ở app/agents/, bạn cần lùi đúng số cấp thư mục để trỏ về data1
default_path = Path(__file__).resolve().parents[2] / "data" / "vector" / "chroma_db_data1"
print("default_path",default_path)
persist_directory = os.getenv("CHROMA_DB_PATH", str(default_path))

vectorstore = Chroma(
    persist_directory=str(persist_directory),
    collection_name="rag_document",
    embedding_function=embeddings  
)

retriever = vectorstore.as_retriever(search_type="similarity", search_kwargs={"k": 5})