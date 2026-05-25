import os
from dotenv import load_dotenv
from langchain_chroma import Chroma
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from pathlib import Path

# Load các biến môi trường từ file .env
load_dotenv() 

# --- Phần dưới giữ nguyên ---
db_path = r"C:\Users\Admin\Documents\Study\Capstone project\backend_ecommerce\ai-module\data\vector\chroma_db_data1"

# Lúc này nó sẽ tự động lấy GOOGLE_API_KEY từ môi trường
embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")
print(db_path)
vectorstore = Chroma(
    persist_directory=str(persist_directory),
    collection_name="sanquo_policy_documents",
    embedding_function=embeddings,
)
retriever = vectorstore.as_retriever(search_type="similarity", search_kwargs={"k": 5})

count = vectorstore._collection.count()
print(f"Tổng số documents đang có trong collection: {count}")