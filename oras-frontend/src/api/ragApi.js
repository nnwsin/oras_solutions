import axiosClient from "./axiosClient";

/**
 * Upload a PDF document to the RAG service.
 * @param {File} file - PDF file to upload.
 * @returns {Promise<Object>} Upload response with document_id, filename, pages, chunks.
 */
export const uploadDocument = async (file) => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await axiosClient.post("/documents/upload", formData, {
        headers: {
            "Content-Type": "multipart/form-data"
        }
    });
    return response.data;
};

/**
 * Fetch all indexed documents.
 * @returns {Promise<Array>} List of documents.
 */
export const getAllDocuments = async () => {
    const response = await axiosClient.get("/documents");
    return response.data?.documents || [];
};

/**
 * Delete a document by ID.
 * @param {string} documentId - UUID of the document.
 * @returns {Promise<Object>} Delete confirmation.
 */
export const deleteDocument = async (documentId) => {
    const response = await axiosClient.delete(`/documents/${documentId}`);
    return response.data;
};

/**
 * Send a user query to the RAG chat endpoint.
 * @param {string} question - The user's query.
 * @param {string} [sessionId] - Optional UUID identifying the chat session for Redis short-term memory.
 * @returns {Promise<Object>} Response containing question, answer, and sources.
 */
export const sendChatMessage = async (question, sessionId = "") => {
    const payload = { question };
    if (sessionId) {
        payload.session_id = sessionId;
    }
    const response = await axiosClient.post("/chat", payload);
    return response.data;
};

/**
 * Clear the Redis short-term session memory for a given sessionId.
 * @param {string} sessionId - UUID of the session to clear.
 * @returns {Promise<Object>}
 */
export const clearChatSession = async (sessionId) => {
    if (!sessionId) return { cleared: true };
    try {
        const response = await axiosClient.delete(`/chat/session/${sessionId}`);
        return response.data;
    } catch (err) {
        console.warn("Failed to clear chat session from Redis:", err);
        return { cleared: false };
    }
};

