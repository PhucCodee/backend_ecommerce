import os
import glob
from typing import List
from pathlib import Path
from dotenv import load_dotenv

# LangChain Imports
from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain_chroma import Chroma
from langchain_core.documents import Document

# Load environment variables (ensure GOOGLE_API_KEY is set)
load_dotenv()

# --- Configuration based on Report Section 6.2 ---
DATA_DIR = Path("document")  # Directory containing PDFs
PERSIST_DIR = Path("chroma_db")   # Vector DB storage location
EMBEDDING_MODEL = "models/text-embedding-004" # 
COLLECTION_NAME = "policies_collection" # [cite: 828]

# Chunking Strategy 
CHUNK_SIZE = 1000
CHUNK_OVERLAP = 200

def load_documents(source_dir: Path) -> List[Document]:
    """
    Scans the directory for PDFs and loads them using PyPDFLoader.
    Ref: Report Section 6.2 - Document Loading 
    """
    if not source_dir.exists():
        raise FileNotFoundError(f"Data directory not found: {source_dir}")
    
    pdf_files = list(source_dir.glob("*.pdf"))
    docs = []
    
    print(f"📂 Found {len(pdf_files)} PDF files in {source_dir}...")
    
    for pdf_path in pdf_files:
        try:
            print(f"   - Loading: {pdf_path.name}")
            # PyPDFLoader extracts text and handles metadata automatically
            loader = PyPDFLoader(str(pdf_path))
            docs.extend(loader.load())
        except Exception as e:
            print(f"❌ Error loading {pdf_path.name}: {e}")
            
    print(f"✅ Loaded {len(docs)} document pages.")
    return docs

def split_documents(documents: List[Document]) -> List[Document]:
    """
    Splits documents into chunks using Recursive Character Text Splitting.
    Ref: Report Section 6.2 - Semantic Chunking Strategy [cite: 820]
    """
    print(f"✂️  Splitting documents (Chunk Size: {CHUNK_SIZE}, Overlap: {CHUNK_OVERLAP})...")
    
    text_splitter = RecursiveCharacterTextSplitter(
        chunk_size=CHUNK_SIZE,
        chunk_overlap=CHUNK_OVERLAP,
        separators=["\n\n", "\n", " ", ""] # Standard recursive separators
    )
    
    chunks = text_splitter.split_documents(documents)
    print(f"✅ Created {len(chunks)} chunks from {len(documents)} pages.")
    return chunks

def save_to_vector_db(chunks: List[Document]):
    """
    Embeds chunks and saves them to ChromaDB.
    Ref: Report Section 6.2 - Vector Storage and Indexing [cite: 824, 827]
    """
    if not chunks:
        print("⚠️ No chunks to ingest.")
        return

    api_key = os.getenv("GOOGLE_API_KEY")
    if not api_key:
        raise ValueError("GOOGLE_API_KEY not found in environment variables.")

    print(f"🧠 Initializing Embedding Model ({EMBEDDING_MODEL})...")
    embeddings = GoogleGenerativeAIEmbeddings(
        model=EMBEDDING_MODEL,
        google_api_key=api_key
    )

    print(f"💾 Persisting to ChromaDB at '{PERSIST_DIR}'...")
    
    # Chroma handles batching automatically
    vectorstore = Chroma.from_documents(
        documents=chunks,
        embedding=embeddings,
        persist_directory=str(PERSIST_DIR),
        collection_name=COLLECTION_NAME
    )
    
    print("✅ Ingestion Complete! Data saved to Vector Database.")

def main():
    print("🚀 Starting Ingestion Pipeline...")
    
    # 1. Load Data
    raw_docs = load_documents(DATA_DIR)
    
    if not raw_docs:
        print("❌ No documents found. Exiting.")
        return

    # 2. Split Text
    chunks = split_documents(raw_docs)

    # 3. Embed & Store
    save_to_vector_db(chunks)

if __name__ == "__main__":
    main()