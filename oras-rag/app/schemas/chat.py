from typing import Literal, Union
from pydantic import BaseModel, Field


class ChatRequest(BaseModel):
    question: str
    session_id: str = ""
    mode: str = "hybrid"


class ClearSessionResponse(BaseModel):
    session_id: str
    cleared: bool


class DocumentSource(BaseModel):
    type: Literal["document"] = "document"
    document_id: str
    filename: str
    page: int
    chunk_index: int


class WebSource(BaseModel):
    type: Literal["web"] = "web"
    url: str
    title: str


# Backwards compatibility alias
SourceResponse = DocumentSource

ChatSource = Union[DocumentSource, WebSource]


class ChatResponse(BaseModel):
    question: str
    answer: str
    sources: list[ChatSource] = Field(default_factory=list)