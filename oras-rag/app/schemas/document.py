from pydantic import BaseModel


class DocumentUploadResponse(BaseModel):
    document_id: str
    filename: str
    pages: int
    chunks: int
    message: str


class DocumentResponse(BaseModel):
    document_id: str
    filename: str


class DocumentListResponse(BaseModel):
    documents: list[DocumentResponse]