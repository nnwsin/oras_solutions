import logging
from dataclasses import dataclass
from typing import Any

from tavily import TavilyClient

from app.core.config import TAVILY_API_KEY, TAVILY_MAX_RESULTS, TAVILY_SEARCH_DEPTH
from app.core.exceptions import WebSearchError

logger = logging.getLogger(__name__)


@dataclass
class WebSearchResult:
    title: str
    url: str
    content: str
    score: float | None = None


class WebSearchService:
    """Independent service for executing web searches via Tavily."""

    def __init__(self, api_key: str | None = None):
        self._api_key = api_key or TAVILY_API_KEY
        self._client: TavilyClient | None = None

        if self._api_key:
            try:
                self._client = TavilyClient(api_key=self._api_key)
            except Exception as e:
                logger.warning(f"Failed to initialize TavilyClient: {e}")

    @property
    def client(self) -> TavilyClient:
        if not self._client:
            if not self._api_key:
                raise WebSearchError("TAVILY_API_KEY is not set. Please add it to your .env file.")
            self._client = TavilyClient(api_key=self._api_key)
        return self._client

    def search(
        self,
        query: str,
        max_results: int | None = None,
        search_depth: str | None = None,
        include_answer: bool = False
    ) -> list[WebSearchResult]:
        """
        Executes a web search for the given query using Tavily.

        :param query: The search query string.
        :param max_results: Maximum number of results to return (default from config: 3).
        :param search_depth: 'basic' or 'advanced' (default from config: 'basic').
        :param include_answer: Whether Tavily should generate an answer snippet.
        :return: List of WebSearchResult instances.
        """
        if not query or not query.strip():
            return []

        limit = max_results if max_results is not None else TAVILY_MAX_RESULTS
        depth = search_depth or TAVILY_SEARCH_DEPTH

        try:
            response: dict[str, Any] = self.client.search(
                query=query.strip(),
                max_results=limit,
                search_depth=depth,
                include_answer=include_answer
            )

            raw_results = response.get("results", [])
            results: list[WebSearchResult] = []

            for item in raw_results:
                results.append(
                    WebSearchResult(
                        title=item.get("title", ""),
                        url=item.get("url", ""),
                        content=item.get("content", ""),
                        score=item.get("score")
                    )
                )

            return results

        except WebSearchError:
            raise
        except Exception as e:
            logger.error(f"Error executing Tavily web search: {e}", exc_info=True)
            raise WebSearchError(str(e)) from e


web_search_service = WebSearchService()
