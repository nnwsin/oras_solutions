from typing import Any

from app.services.search_orchestrator import search_orchestrator


def ask_question(question: str, mode: str = "hybrid") -> dict[str, Any]:
    """
    Answers a question by querying documents and/or the web via SearchOrchestrator.

    :param question: The question to answer.
    :param mode: 'hybrid', 'documents_only', or 'web_only'.
    :return: Dictionary containing 'answer' and 'sources'.
    """
    return search_orchestrator.ask_question(question=question, mode=mode)