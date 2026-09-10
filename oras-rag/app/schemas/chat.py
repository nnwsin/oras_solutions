from pydantic import BaseModel


class ChatRequest(BaseModel):
    question: str


class SourceResponse(BaseModel):
    document_id: str
    filename: str
    page: int
    chunk_index: int


class ChatResponse(BaseModel):
    question: str
    answer: str
    sources: list[SourceResponse]