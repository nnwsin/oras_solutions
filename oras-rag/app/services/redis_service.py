import json
import logging
import time
from typing import Any

import redis

from app.core.config import (
    REDIS_DB,
    REDIS_HOST,
    REDIS_PASSWORD,
    REDIS_PORT,
    REDIS_SESSION_TTL,
    REDIS_URL,
)

logger = logging.getLogger(__name__)


class RedisService:
    """
    Manages short-term conversational chat history in Redis.
    Each session is keyed as `chat:session:{session_id}`.
    """

    def __init__(self):
        self._client: redis.Redis | None = None
        self._init_client()

    def _init_client(self) -> None:
        try:
            if REDIS_URL:
                self._client = redis.Redis.from_url(
                    REDIS_URL,
                    decode_responses=True,
                    socket_connect_timeout=3,
                )
            else:
                self._client = redis.Redis(
                    host=REDIS_HOST,
                    port=REDIS_PORT,
                    password=REDIS_PASSWORD,
                    db=REDIS_DB,
                    decode_responses=True,
                    socket_connect_timeout=3,
                )
            # Test connection
            self._client.ping()
            logger.info("Connected to Redis successfully.")
        except Exception as e:
            logger.warning(f"Redis connection initialization failed: {e}. Chat memory will operate in degraded mode.")
            self._client = None

    @property
    def client(self) -> redis.Redis | None:
        if self._client is None:
            self._init_client()
        return self._client

    def _get_key(self, session_id: str) -> str:
        return f"chat:session:{session_id}"

    def get_session_history(self, session_id: str, limit: int = 10) -> list[dict[str, Any]]:
        """
        Retrieves the last `limit` conversation turns for the given session_id.
        Returns a list of dicts: [{'role': 'user'|'assistant', 'content': '...'}]
        """
        if not session_id or not self.client:
            return []

        key = self._get_key(session_id)
        try:
            raw_entries = self.client.lrange(key, -limit, -1)
            history = []
            for entry in raw_entries:
                try:
                    data = json.loads(entry)
                    if "role" in data and "content" in data:
                        history.append({
                            "role": data["role"],
                            "content": data["content"]
                        })
                except json.JSONDecodeError:
                    continue
            return history
        except Exception as e:
            logger.warning(f"Failed to fetch session history from Redis for {session_id}: {e}")
            return []

    def append_session_messages(
        self,
        session_id: str,
        user_message: str,
        assistant_message: str
    ) -> None:
        """
        Appends user and assistant messages to the Redis list and refreshes the TTL.
        """
        if not session_id or not self.client:
            return

        key = self._get_key(session_id)
        now = time.time()
        user_entry = json.dumps({"role": "user", "content": user_message, "timestamp": now})
        assistant_entry = json.dumps({"role": "assistant", "content": assistant_message, "timestamp": now})

        try:
            pipe = self.client.pipeline()
            pipe.rpush(key, user_entry, assistant_entry)
            pipe.expire(key, REDIS_SESSION_TTL)
            pipe.execute()
        except Exception as e:
            logger.warning(f"Failed to append session messages to Redis for {session_id}: {e}")

    def delete_session(self, session_id: str) -> bool:
        """
        Immediately deletes the session key from Redis.
        """
        if not session_id or not self.client:
            return True

        key = self._get_key(session_id)
        try:
            self.client.delete(key)
            logger.info(f"Purged session history from Redis for session: {session_id}")
            return True
        except Exception as e:
            logger.warning(f"Failed to delete session {session_id} from Redis: {e}")
            return False


redis_service = RedisService()
