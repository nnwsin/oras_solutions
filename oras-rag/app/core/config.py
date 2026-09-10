import os
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()

# Base project directory (oras-rag root)
BASE_DIR = Path(__file__).resolve().parent.parent.parent

# Data Directories
DATA_DIR = BASE_DIR / "data"
DOCUMENTS_DIR = DATA_DIR / "documents"
CHROMA_DIR = DATA_DIR / "chroma"
METADATA_FILE = DATA_DIR / "metadata.json"

# Ensure required directories exist
DOCUMENTS_DIR.mkdir(parents=True, exist_ok=True)
CHROMA_DIR.mkdir(parents=True, exist_ok=True)

# Gemini & Vector Store Configurations
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
if not GEMINI_API_KEY:
    raise RuntimeError(
        "GEMINI_API_KEY is not set. "
        "Add it to your .env file: GEMINI_API_KEY=your_key_here"
    )
LLM_MODEL = os.getenv("LLM_MODEL", "gemini-3.6-flash")
EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "gemini-embedding-001")
CHROMA_COLLECTION_NAME = "oras_documents"

# Text Splitter Configurations
CHUNK_SIZE = 1000
CHUNK_OVERLAP = 200

# CORS Allowed Origins
CORS_ORIGINS = [
    "http://localhost:3000",
    "http://localhost:5173",
    "http://127.0.0.1:3000",
    "http://127.0.0.1:5173",
]
