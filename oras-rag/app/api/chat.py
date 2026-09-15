from fastapi import APIRouter

from app.schemas.chat import ChatRequest, ChatResponse, ClearSessionResponse
from app.services.rag_service import ask_question
from app.services.redis_service import redis_service

router = APIRouter(
    prefix="/chat",
    tags=["Chat"]
)


@router.post("", response_model=ChatResponse)
def chat(request: ChatRequest):
    result = ask_question(
        question=request.question,
        session_id=request.session_id,
        mode=request.mode
    )

    return ChatResponse(
        question=request.question,
        answer=result["answer"],
        sources=result["sources"]
    )


@router.delete("/session/{session_id}", response_model=ClearSessionResponse)
def clear_session(session_id: str):
    cleared = redis_service.delete_session(session_id)
    return ClearSessionResponse(session_id=session_id, cleared=cleared)