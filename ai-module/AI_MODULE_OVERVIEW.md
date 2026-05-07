# AI Module - Project Overview

## 🎯 Purpose
The AI Module is a microservice that provides intelligent conversational AI capabilities for the e-commerce platform. It handles:
- **Buyer Support**: Product recommendations, FAQ, order tracking, returns
- **Seller Tools**: Inventory management, sales analytics, customer communication
- **Admin Dashboard**: Analytics, user management, policy updates

---

## 🏗️ Architecture Overview

### Tech Stack
| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | FastAPI | Latest |
| **AI Orchestration** | LangGraph | Latest |
| **AI Chain Framework** | LangChain | 0.x |
| **LLM Provider** | Groq (llama-3.3-70b) | Latest |
| **Embeddings** | Google Gemini | text-embedding-001 |
| **Vector Database** | ChromaDB | Latest |
| **SQL Database** | PostgreSQL | 13+ |
| **Server** | Uvicorn | Latest |

### Key Features
- ✅ Multi-agent system (Buyer, Seller, Admin)
- ✅ RAG (Retrieval-Augmented Generation) for document retrieval
- ✅ SQL Agent for database queries
- ✅ Vector store for document embeddings
- ✅ Stateful chat sessions
- ✅ Role-based agent routing

---

## 📁 Project Structure

```
ai-module/
├── Dockerfile                 # Docker configuration
├── requirements.txt           # Python dependencies
├── .env                       # Environment variables (local)
├── README.md                  # Quick start guide
│
├── app/
│   ├── api.py                 # FastAPI application (main entry point)
│   ├── agents/                 # Agent implementations
│   │   └── subgraphs/            
│   │   │   ├── orchestrator.py      # Main orchestrator
│   │   │   ├── orchestrator_new.py  # Updated version
│   │   │   ├── prompts.py           # System prompts
│   │   │   ├── rag_agent.py         # RAG subgraph
│   │   │   ├── sql_agent.py         # SQL subgraph
│   │   │   └── subgraphs/           # (Currently empty)
│   └── core/
│       └── state.py          # MasterState definition
│
├── data/
│   ├── document/             # Raw documents for RAG
│   └── vector/               # ChromaDB persistent storage
│       └── chroma.sqlite3
│
└── main/
    └── pre_processing.ipynb  # Data preprocessing scripts
```

---

## 🔄 Agent Workflow

### 1. Request Flow
```
User Request
    ↓
[FastAPI /api/ai/chat endpoint]
    ↓
[MasterState initialization]
    ↓
[Orchestrator routes to appropriate agent]
    ↓
[Agent executes: RAG or SQL Agent]
    ↓
[Response generation]
    ↓
[Return ChatResponse]
```

### 2. Agent Types

#### **Buyer Agent**
- Handles customer inquiries
- Provides product recommendations
- Retrieves policy information via RAG
- Queries order/product data via SQL

#### **Seller Agent**
- Shop management assistance
- Inventory queries
- Sales analytics
- Product listing help

#### **Admin Agent**
- System analytics
- User management
- Policy updates
- Store configuration

---

## 🚀 Quick Start

### Local Setup
```bash
cd ai-module
pip install -r requirements.txt
python -m app.api
# Server starts at http://localhost:8000
```

### Docker Deployment
```bash
docker build -t ai-service .
docker run -p 8000:8000 ai-service
```

---

## 📊 Data Flow

### Vector Store (ChromaDB)
- Stores embeddings of policy documents
- Used by RAG Agent for context retrieval
- Location: `./data/vector/`

### SQL Database (PostgreSQL)
- Stores product, order, user data
- Connected via `psycopg2`
- Accessed by SQL Agent

### Session Management
- Each conversation has a unique `session_id`
- State persisted using LangGraph's state management
- Supports multi-turn conversations

---

## 📝 Configuration

### Environment Variables (.env)
```
# LLM Provider
GROQ_API_KEY=your_groq_key

# Embeddings Provider
GOOGLE_API_KEY=your_google_key

# Database
DATABASE_URL=postgresql://user:pass@localhost/dbname
CHROMA_DB_PATH=./data/vector

# Server
ENVIRONMENT=development
LOG_LEVEL=INFO
```

---

## 🔗 Integration Points

### Backend API
- Receives user requests from frontend
- Calls `/api/ai/chat` endpoint
- Includes user context (role, user_id, email)

### Frontend
- Buyer: Chat interface for product help
- Seller: Shop assistant panel
- Admin: Analytics & management console

---

## 📚 Documentation Files

1. **ARCHITECTURE.md** - Detailed technical architecture
2. **AGENTS_DEVELOPMENT_GUIDE.md** - How to develop new agents
3. **SETUP_AND_DEPLOYMENT.md** - Complete setup instructions
4. **VECTOR_STORE_MANAGEMENT.md** - Managing RAG documents
5. **API_REFERENCE.md** - Endpoint documentation
6. **TROUBLESHOOTING.md** - Common issues & fixes
7. **TESTING_GUIDE.md** - Testing strategies

---

## 🎓 Next Steps

1. **Review** ARCHITECTURE.md for deep dive
2. **Set up** locally using SETUP_AND_DEPLOYMENT.md
3. **Explore** AGENTS_DEVELOPMENT_GUIDE.md for customization
4. **Test** using TESTING_GUIDE.md
5. **Deploy** following SETUP_AND_DEPLOYMENT.md

---

## 🔗 Useful Links
- [LangGraph Docs](https://python.langchain.com/docs/langgraph/)
- [LangChain Docs](https://python.langchain.com/)
- [ChromaDB Docs](https://docs.trychroma.com/)
- [FastAPI Docs](https://fastapi.tiangolo.com/)
