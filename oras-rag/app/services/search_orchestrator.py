import logging
from concurrent.futures import ThreadPoolExecutor
from typing import Any

from langchain_google_genai import ChatGoogleGenerativeAI

from app.core.config import LLM_MODEL, TAVILY_MAX_RESULTS
from app.core.exceptions import DocumentProcessingError, WebSearchError
from app.services.vector_store import vector_store
from app.services.web_search_service import WebSearchResult, web_search_service

logger = logging.getLogger(__name__)

llm = ChatGoogleGenerativeAI(
    model=LLM_MODEL,
    temperature=0
)


class SearchOrchestrator:
    """
    Orchestrates search and answer synthesis across internal documents (vector store)
    and external live web search (Tavily), with an intelligent query router.
    """

    def __init__(self):
        self.llm = llm

    @staticmethod
    def _extract_text(content: Any) -> str:
        """Helper to extract plain string from LLM response content."""
        if isinstance(content, list):
            parts = []
            for block in content:
                if isinstance(block, dict) and "text" in block:
                    parts.append(block["text"])
                elif isinstance(block, str):
                    parts.append(block)
            return "".join(parts)
        elif not isinstance(content, str):
            return str(content)
        return content

    def _classify_query(self, question: str) -> str:
        """
        Classifies the user query into one of: 'general', 'internal', 'web_needed', or 'hybrid'.
        Falls back to 'hybrid' on any failure or unrecognized classification.
        """
        try:
            doc_names = []
            try:
                from app.services.document_service import load_metadata
                metadata = load_metadata()
                doc_names = [info.get("filename", "") for info in metadata.values() if info.get("filename")]
            except Exception as e:
                logger.debug(f"Could not load document metadata for query routing: {e}")

            docs_context = ""
            if doc_names:
                docs_context = f"\nCurrently uploaded internal documents: {', '.join(doc_names)}\n"

            classification_prompt = f"""You are a query classification router for an enterprise AI assistant.
Classify the following user question into exactly ONE of these four categories:

1. general: Common knowledge questions, definitions, programming concepts, math, language, science, history, or general conversation that do not require internal company documents or live web search.
2. internal: Questions specifically about company documents, policies, internal processes, HR, or topics related to uploaded documents.{docs_context}
3. web_needed: Questions that strictly require real-time or current web information (e.g., today's news, current weather, recent sports scores, latest market data, up-to-date live events).
4. hybrid: Questions requiring both internal company document context and external web comparison (e.g., comparing company policy against external industry standards).

Respond with ONLY ONE word: 'general', 'internal', 'web_needed', or 'hybrid'. Do not provide any explanation, markdown, or punctuation.

Question: {question}
Category:"""

            response = self.llm.invoke(classification_prompt)
            raw_category = self._extract_text(response.content).strip().lower()
            category = "".join(c for c in raw_category if c.isalnum() or c == "_")

            if category in {"general", "internal", "web_needed", "hybrid"}:
                logger.info(f"Query classified as: {category}")
                return category

            logger.warning(f"Router returned unexpected category '{raw_category}', falling back to 'hybrid'")
            return "hybrid"
        except Exception as e:
            logger.warning(f"Query classification failed: {e}. Falling back to 'hybrid'")
            return "hybrid"

    def _retrieve_documents(self, question: str, k: int = 5, score_threshold: float = 0.40) -> list[Any]:
        try:
            results_with_scores = vector_store.similarity_search_with_relevance_scores(question, k=k)
            # Only keep documents meeting the relevance threshold
            return [doc for doc, score in results_with_scores if score >= score_threshold]
        except Exception as e:
            logger.warning(f"similarity_search_with_relevance_scores failed, falling back to similarity_search: {e}")
            try:
                return vector_store.similarity_search(question, k=k)
            except Exception as err:
                logger.error(f"Error retrieving documents from vector store: {err}", exc_info=True)
                return []

    def _retrieve_web(self, question: str, max_results: int = TAVILY_MAX_RESULTS) -> list[WebSearchResult]:
        try:
            return web_search_service.search(query=question, max_results=max_results)
        except WebSearchError as e:
            logger.warning(f"Web search failed in orchestrator: {e}")
            return []
        except Exception as e:
            logger.warning(f"Unexpected error during web search: {e}")
            return []

    def _answer_general(self, question: str) -> dict[str, Any]:
        """Directly answers general knowledge questions using LLM's intrinsic knowledge."""
        prompt = f"""You are the ORAAS AI assistant.
Answer the following question accurately, concisely, and helpfully using your general knowledge.

Question:
{question}
"""
        try:
            response = self.llm.invoke(prompt)
            answer = self._extract_text(response.content)
            return {
                "answer": answer,
                "sources": []
            }
        except Exception as e:
            logger.error(f"Error answering general query: {e}", exc_info=True)
            raise DocumentProcessingError(f"Error generating answer: {str(e)}") from e

    def ask_question(
        self,
        question: str,
        mode: str = "hybrid"
    ) -> dict[str, Any]:
        """
        Answers a user question using internal documents, web search, or both.

        :param question: The user query.
        :param mode: 'hybrid' (uses router to pick best strategy), 'documents_only', or 'web_only'.
        :return: Dict with 'answer' and 'sources'.
        """
        documents = []
        web_results = []

        # If mode is explicitly set by caller, honor it without routing
        if mode in ("documents_only", "web_only"):
            effective_mode = mode
        else:
            effective_mode = self._classify_query(question)

        logger.info(f"Effective search mode for question '{question}': {effective_mode}")

        # If general knowledge, answer directly without retrieval
        if effective_mode == "general":
            return self._answer_general(question)

        # Retrieve documents or web or both
        if effective_mode in ("documents_only", "internal"):
            documents = self._retrieve_documents(question)
        elif effective_mode in ("web_only", "web_needed"):
            web_results = self._retrieve_web(question)
        else:  # hybrid or fallback
            with ThreadPoolExecutor(max_workers=2) as executor:
                doc_future = executor.submit(self._retrieve_documents, question)
                web_future = executor.submit(self._retrieve_web, question)
                documents = doc_future.result()
                web_results = web_future.result()

        context_blocks = []
        sources: list[dict[str, Any]] = []

        # Format Document Context & Sources
        if documents:
            doc_context_parts = []
            for doc in documents:
                meta = doc.metadata or {}
                filename = meta.get("filename", "Unknown")
                page = meta.get("page", 0) + 1
                chunk_index = meta.get("chunk_index", 0)
                doc_id = meta.get("document_id", "")

                doc_context_parts.append(f"[{filename} (Page {page})]:\n{doc.page_content}")

                sources.append({
                    "type": "document",
                    "document_id": doc_id,
                    "filename": filename,
                    "page": page,
                    "chunk_index": chunk_index
                })

            context_blocks.append(
                "### Context from Uploaded Documents:\n" + "\n\n".join(doc_context_parts)
            )

        # Format Web Context & Sources
        if web_results:
            web_context_parts = []
            for item in web_results:
                web_context_parts.append(f"[{item.title} ({item.url})]:\n{item.content}")

                sources.append({
                    "type": "web",
                    "url": item.url,
                    "title": item.title or item.url
                })

            context_blocks.append(
                "### Context from Web Search:\n" + "\n\n".join(web_context_parts)
            )

        combined_context = "\n\n----------------\n\n".join(context_blocks) if context_blocks else "No relevant context found."

        prompt = f"""You are the ORAAS AI assistant.

Answer the user's question accurately and helpfully using the context provided below.
You have access to uploaded internal document chunks and/or live web search results.

Context:
----------------
{combined_context}
----------------

Instructions:
1. Answer using the information provided in the context above.
2. If internal documents provide relevant information, prioritize them for internal/company-specific questions.
3. If web search provides relevant information, use it to answer general or current questions.
4. Clearly synthesize information if both sources are relevant.
5. If the context does not contain enough information to answer the question, state clearly that you do not have enough information.
6. Do not fabricate or invent facts that cannot be supported by the provided context.

Question:
{question}
"""

        try:
            response = self.llm.invoke(prompt)
            answer = self._extract_text(response.content)

            return {
                "answer": answer,
                "sources": sources
            }

        except Exception as e:
            logger.error(f"Error executing LLM generation in orchestrator: {e}", exc_info=True)
            raise DocumentProcessingError(f"Error generating answer: {str(e)}") from e


search_orchestrator = SearchOrchestrator()

