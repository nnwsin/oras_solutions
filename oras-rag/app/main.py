from dotenv import load_dotenv

# Load .env before any other imports so os.getenv() calls in config.py work correctly
load_dotenv()

from fastapi import FastAPI

from app.api.chat import router as chat_router
from app.api.documents import router as documents_router
from app.core.exceptions import register_exception_handlers

app = FastAPI(
    title="ORAAS RAG Service",
    description="AI and RAG service for ORAAS",
    version="1.0.0"
)

# Register custom exception handlers (DuplicateDocumentError, DocumentNotFoundError, etc.)
register_exception_handlers(app)

# All routes live under /api — e.g. /api/documents/upload, /api/chat
app.include_router(documents_router, prefix="/api")
app.include_router(chat_router, prefix="/api")


@app.get("/")
def root():
    return {
        "message": "ORAAS RAG Service is running"
    }


@app.get("/health")
def health():
    return {
        "status": "healthy"
    }