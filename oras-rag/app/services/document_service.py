import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter

from app.core.config import CHUNK_OVERLAP, CHUNK_SIZE, DOCUMENTS_DIR, METADATA_FILE
from app.core.exceptions import (
    DocumentNotFoundError,
    DocumentProcessingError,
    DuplicateDocumentError,
)
from app.services.vector_store import vector_store


def load_metadata() -> dict[str, dict[str, Any]]:
    """Loads document metadata dictionary from JSON file."""
    if not METADATA_FILE.exists():
        return {}
    try:
        with METADATA_FILE.open("r", encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def save_metadata(metadata: dict[str, dict[str, Any]]) -> None:
    """Saves document metadata dictionary to JSON file."""
    METADATA_FILE.parent.mkdir(parents=True, exist_ok=True)
    with METADATA_FILE.open("w", encoding="utf-8") as f:
        json.dump(metadata, f, indent=2)


def compute_file_hash(file_path: Path) -> str:
    """Computes SHA-256 hash of a file using streaming reads (memory-efficient)."""
    hasher = hashlib.sha256()
    with file_path.open("rb") as f:
        while chunk := f.read(8192):
            hasher.update(chunk)
    return hasher.hexdigest()


def check_duplicate(file_hash: str, filename: str) -> None:
    """Raises DuplicateDocumentError if a document with the given file hash already exists."""
    metadata = load_metadata()
    for doc_id, doc_info in metadata.items():
        if doc_info.get("file_hash") == file_hash:
            raise DuplicateDocumentError(
                filename=filename,
                existing_document_id=doc_id
            )


def process_document(
    file_path: Path,
    document_id: str,
    filename: str,
    file_hash: str | None = None
) -> dict[str, Any]:
    """Processes a PDF: loads pages, splits into chunks, indexes in Chroma, and saves metadata."""
    try:
        loader = PyPDFLoader(str(file_path))
        documents = loader.load()

        splitter = RecursiveCharacterTextSplitter(
            chunk_size=CHUNK_SIZE,
            chunk_overlap=CHUNK_OVERLAP
        )

        chunks = splitter.split_documents(documents)

        for index, chunk in enumerate(chunks):
            chunk.metadata["document_id"] = document_id
            chunk.metadata["filename"] = filename
            chunk.metadata["chunk_index"] = index

        vector_store.add_documents(chunks)

        if not file_hash:
            file_hash = compute_file_hash(file_path)

        metadata = load_metadata()
        metadata[document_id] = {
            "document_id": document_id,
            "filename": filename,
            "file_hash": file_hash,
            "pages": len(documents),
            "chunks": len(chunks),
            "uploaded_at": datetime.now(timezone.utc).isoformat()
        }
        save_metadata(metadata)

        return {
            "document_id": document_id,
            "filename": filename,
            "pages": len(documents),
            "chunks": len(chunks)
        }
    except Exception as e:
        if not isinstance(e, (DuplicateDocumentError, DocumentProcessingError)):
            raise DocumentProcessingError(str(e))
        raise


def delete_document(document_id: str) -> None:
    """Deletes a document from vector store, metadata registry, and file system."""
    metadata = load_metadata()
    found_in_metadata = document_id in metadata

    # 1. Delete chunks from vector store
    results = vector_store.get(
        where={"document_id": document_id}
    )
    ids = results.get("ids", [])
    if ids:
        vector_store.delete(ids=ids)

    # 2. Delete PDF file from disk
    file_deleted = False
    for file_path in DOCUMENTS_DIR.glob(f"{document_id}.*"):
        if file_path.exists():
            file_path.unlink()
            file_deleted = True

    # 3. Remove entry from metadata registry
    if found_in_metadata:
        del metadata[document_id]
        save_metadata(metadata)

    if not ids and not file_deleted and not found_in_metadata:
        raise DocumentNotFoundError(document_id)


def list_documents() -> list[dict[str, Any]]:
    """Returns the list of all registered documents from the metadata registry."""
    metadata = load_metadata()
    if metadata:
        return [
            {
                "document_id": info["document_id"],
                "filename": info["filename"]
            }
            for info in metadata.values()
        ]

    # Fallback: scan vector store for documents uploaded before metadata.json existed
    results = vector_store.get(include=["metadatas"])
    documents: dict[str, dict[str, Any]] = {}

    for meta in results.get("metadatas", []):
        if not meta:
            continue
        doc_id = meta.get("document_id")
        filename = meta.get("filename")
        if doc_id and doc_id not in documents:
            documents[doc_id] = {
                "document_id": doc_id,
                "filename": filename
            }

    return list(documents.values())