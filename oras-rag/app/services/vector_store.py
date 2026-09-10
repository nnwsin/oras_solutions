from langchain_chroma import Chroma
from langchain_google_genai import GoogleGenerativeAIEmbeddings

from app.core.config import CHROMA_COLLECTION_NAME, CHROMA_DIR, EMBEDDING_MODEL

embeddings = GoogleGenerativeAIEmbeddings(
    model=EMBEDDING_MODEL
)

vector_store = Chroma(
    collection_name=CHROMA_COLLECTION_NAME,
    embedding_function=embeddings,
    persist_directory=str(CHROMA_DIR)
)