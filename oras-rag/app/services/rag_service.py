from langchain_google_genai import ChatGoogleGenerativeAI

from app.core.config import LLM_MODEL
from app.core.exceptions import DocumentProcessingError
from app.services.vector_store import vector_store

llm = ChatGoogleGenerativeAI(
    model=LLM_MODEL,
    temperature=0
)


def ask_question(question: str):
    try:
        documents = vector_store.similarity_search(
            question,
            k=5
        )

        context_parts = []
        sources = []

        for document in documents:
            context_parts.append(
                document.page_content
            )

            sources.append({
                "document_id": document.metadata.get(
                    "document_id",
                    ""
                ),
                "filename": document.metadata.get(
                    "filename",
                    ""
                ),
                "page": document.metadata.get(
                    "page",
                    0
                ) + 1,
                "chunk_index": document.metadata.get(
                    "chunk_index",
                    0
                )
            })

        context = "\n\n".join(context_parts)

        prompt = f"""
You are the ORAAS AI assistant.

Answer the user's question using ONLY the information
provided in the context below.

If the answer cannot be found in the context,
say that you don't have enough information.

Do not make up information.

Context:
----------------
{context}
----------------

Question:
{question}
"""

        response = llm.invoke(prompt)

        return {
            "answer": response.content,
            "sources": sources
        }
    except Exception as e:
        if not isinstance(e, DocumentProcessingError):
            raise DocumentProcessingError(f"Error executing RAG query: {str(e)}")
        raise