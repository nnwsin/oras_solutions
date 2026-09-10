from pathlib import Path
import shutil
import uuid

from fastapi import APIRouter, File, UploadFile

from app.core.config import DOCUMENTS_DIR
from app.core.exceptions import UnsupportedFileTypeError
from app.schemas.document import (
    DocumentListResponse,
    DocumentUploadResponse,
)
from app.services.document_service import (
    check_duplicate,
    compute_file_hash,
    delete_document,
    list_documents,
    process_document,
)

router = APIRouter(
    prefix="/documents",
    tags=["Documents"]
)


@router.post(
    "/upload",
    response_model=DocumentUploadResponse
)
def upload_document(
    file: UploadFile = File(...)
):
    if not file.filename:
        raise UnsupportedFileTypeError(filename="Unknown", extension="")

    file_extension = Path(file.filename).suffix.lower()
    if file_extension != ".pdf":
        raise UnsupportedFileTypeError(filename=file.filename, extension=file_extension)

    document_id = str(uuid.uuid4())
    saved_filename = f"{document_id}{file_extension}"
    file_path = DOCUMENTS_DIR / saved_filename

    # Save uploaded file to disk
    with file_path.open("wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    # Compute SHA-256 hash & check duplicate
    try:
        file_hash = compute_file_hash(file_path)
        check_duplicate(file_hash, file.filename)
    except Exception as e:
        # Cleanup uploaded file on duplicate or error
        if file_path.exists():
            file_path.unlink()
        raise e

    # Process and index document
    result = process_document(
        file_path=file_path,
        document_id=document_id,
        filename=file.filename,
        file_hash=file_hash
    )

    return {
        "message": "Document processed successfully",
        **result
    }


@router.get(
    "",
    response_model=DocumentListResponse
)
def get_documents():
    documents = list_documents()
    return {
        "documents": documents
    }


@router.delete("/{document_id}")
def remove_document(
    document_id: str
):
    delete_document(document_id)
    return {
        "document_id": document_id,
        "message": "Document deleted successfully"
    }