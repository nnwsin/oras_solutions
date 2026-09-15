from typing import Any

from app.services.search_orchestrator import search_orchestrator


def ask_question(question: str, session_id: str = "", mode: str = "hybrid") -> dict[str, Any]:
    """
    Answers a question by querying documents and/or the web via SearchOrchestrator,
    maintaining short-term conversational context in Redis.

    :param question: The question to answer.
    :param session_id: Optional UUID identifying the current active chat session.
    :param mode: 'hybrid', 'documents_only', or 'web_only'.
    :return: Dictionary containing 'answer' and 'sources'.
    """
    return search_orchestrator.ask_question(question=question, session_id=session_id, mode=mode)