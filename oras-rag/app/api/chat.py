from fastapi import APIRouter

from app.schemas.chat import ChatRequest, ChatResponse
from app.services.rag_service import ask_question

router = APIRouter(
    prefix="/chat",
    tags=["Chat"]
)


@router.post("", response_model=ChatResponse)
def chat(request: ChatRequest):
    result = ask_question(request.question)

    return ChatResponse(
        question=request.question,
        answer=result["answer"],
        sources=result["sources"]
    )