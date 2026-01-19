import os
from pydantic_settings import BaseSettings
from langchain.chat_models import init_chat_model

class Settings(BaseSettings):
    GOOGLE_API_KEY: str
    GROQ_API_KEY: str
    DB_HOST: str = "localhost"
    DB_NAME: str = "database"
    DB_USER: str = "postgres"
    DB_PASS: str = "123"
    DB_PORT: str = "5432"
    
    class Config:
        env_file = ".env"

settings = Settings()

def get_llm():
    """Returns the configured LLM instance."""
    return init_chat_model(
        "llama-3.3-70b-versatile", 
        model_provider="groq", 
        temperature=0
    )