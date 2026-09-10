from fastapi import FastAPI, Request, status
from fastapi.responses import JSONResponse


class OrasBaseException(Exception):
    """Base exception for ORAAS RAG Service."""
    pass


class DocumentNotFoundError(OrasBaseException):
    def __init__(self, document_id: str):
        self.document_id = document_id
        self.message = f"Document with ID '{document_id}' was not found."
        super().__init__(self.message)


class DuplicateDocumentError(OrasBaseException):
    def __init__(self, filename: str, existing_document_id: str):
        self.filename = filename
        self.existing_document_id = existing_document_id
        self.message = (
            f"Document '{filename}' has already been uploaded "
            f"(Document ID: {existing_document_id})."
        )
        super().__init__(self.message)


class UnsupportedFileTypeError(OrasBaseException):
    def __init__(self, filename: str, extension: str):
        self.filename = filename
        self.extension = extension
        self.message = f"File '{filename}' with extension '{extension}' is not supported. Only PDF files are allowed."
        super().__init__(self.message)


class DocumentProcessingError(OrasBaseException):
    def __init__(self, detail: str):
        self.detail = detail
        self.message = f"Failed to process document: {detail}"
        super().__init__(self.message)


def register_exception_handlers(app: FastAPI) -> None:
    @app.exception_handler(DocumentNotFoundError)
    async def document_not_found_handler(request: Request, exc: DocumentNotFoundError):
        return JSONResponse(
            status_code=status.HTTP_404_NOT_FOUND,
            content={"detail": exc.message, "document_id": exc.document_id}
        )

    @app.exception_handler(DuplicateDocumentError)
    async def duplicate_document_handler(request: Request, exc: DuplicateDocumentError):
        return JSONResponse(
            status_code=status.HTTP_409_CONFLICT,
            content={
                "detail": exc.message,
                "filename": exc.filename,
                "existing_document_id": exc.existing_document_id
            }
        )

    @app.exception_handler(UnsupportedFileTypeError)
    async def unsupported_file_type_handler(request: Request, exc: UnsupportedFileTypeError):
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"detail": exc.message}
        )

    @app.exception_handler(DocumentProcessingError)
    async def document_processing_handler(request: Request, exc: DocumentProcessingError):
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={"detail": exc.message}
        )
