# ORAAS RAG Service

Retrieval-Augmented Generation (RAG) backend service for ORAAS built with **FastAPI**, **LangChain**, and **ChromaDB**.

## Project Architecture

```
oras-rag/
├── app/
│   ├── main.py                    # FastAPI application initialization & middleware
│   ├── api/                       # API Route handlers
│   │   ├── chat.py                # Chat / Q&A endpoint
│   │   └── documents.py           # Document upload, list, and delete endpoints
│   ├── core/                      # Application core configuration & exceptions
│   │   ├── config.py              # Centralized settings & paths
│   │   └── exceptions.py          # Custom exception classes & HTTP error handlers
│   ├── schemas/                   # Pydantic data schemas
│   │   ├── chat.py
│   │   └── document.py
│   └── services/                  # Business logic
│       ├── document_service.py    # PDF processing, SHA-256 duplicate check & metadata storage
│       ├── rag_service.py         # LLM retrieval & context assembly
│       └── vector_store.py        # ChromaDB vector store initialization
└── data/                          # Data directory (Git ignored)
    ├── documents/                 # PDF files uploaded by users
    ├── chroma/                    # Persistent vector database files
    └── metadata.json              # Document registry with SHA-256 hashes
```

---

## Features

- **Document Processing**: Parses PDF documents into semantic chunks using `RecursiveCharacterTextSplitter`.
- **SHA-256 Duplicate Prevention**: Prevents uploading duplicate files by checking SHA-256 hashes before indexing.
- **Persistent Metadata Registry**: Keeps track of document IDs, original filenames, hashes, page counts, chunk counts, and upload timestamps in `data/metadata.json`.
- **Vector Search & Grounded Generation**: Uses Google Gemini embeddings and Gemini Flash LLM to answer user questions grounded strictly in context.
- **Clean Exception Architecture**: Converts domain exceptions (`DuplicateDocumentError`, `DocumentNotFoundError`, `UnsupportedFileTypeError`) to standard HTTP responses (`409`, `404`, `400`).
- **CORS Support**: Ready for integration with `oras-frontend`.

---

## Getting Started

### Prerequisites

- Python `>= 3.14` (or compatible version via `uv`)
- `GEMINI_API_KEY` set in `.env` file

### Running the Server

```bash
uv run uvicorn app.main:app --reload
```

The API will be available at `http://127.0.0.1:8000`.

Interactive Swagger docs will be at `http://127.0.0.1:8000/docs`.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | Health check endpoint |
| `POST` | `/documents/upload` | Upload a PDF document (SHA-256 duplicate check enabled) |
| `GET` | `/documents` | List all processed documents |
| `DELETE` | `/documents/{document_id}` | Delete a document from vector store, metadata, and disk |
| `POST` | `/chat` | Ask a question based on uploaded document context |

All document and chat endpoints are also exposed under the `/api` prefix (e.g. `/api/documents/upload`, `/api/chat`).
