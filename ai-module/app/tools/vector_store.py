# app/tools/vector_store.py
import os
from langchain_chroma import Chroma
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from app.core.config import settings

# Global singleton to prevent reloading on every request
_vectorstore = None

def get_vectorstore():
    global _vectorstore
    if _vectorstore is None:
        embeddings = GoogleGenerativeAIEmbeddings(
            model="models/text-embedding-004",
            google_api_key=settings.GOOGLE_API_KEY
        )
        # Assumes you ran the ingest script and the DB exists in ./chroma_db
        # Update 'persist_directory' to match where your script saves it
        _vectorstore = Chroma(
            persist_directory=".../vector/chroma_db", 
            collection_name="stock_market",
            embedding_function=embeddings
        )
    return _vectorstore

def retrieve_documents(query: str, k: int = 4):
    """Simple wrapper to get text content from vector store"""
    vs = get_vectorstore()
    # Fetch top K most relevant docs
    docs = vs.similarity_search(query, k=k)
    # Return just the content joined together
    return "\n\n".join([d.page_content for d in docs])