import { useState, useRef, useEffect } from "react";
import { sendChatMessage } from "../api/ragApi";
import { useAuth } from "../context/AuthContext";

const SUGGESTED_PROMPTS = [
    "What are the main topics covered in the uploaded documents?",
    "Summarize the key action items and deliverables.",
    "What guidelines or policies are outlined here?"
];

const Chat = () => {
    const { user, userEmail } = useAuth();
    const [messages, setMessages] = useState([
        {
            id: "welcome-1",
            sender: "assistant",
            text: "Hello! I am your ORAS AI Assistant. You can ask me questions about any uploaded documents, and I'll retrieve answers grounded in the document knowledge base.",
            sources: [],
            timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
        }
    ]);
    const [inputQuery, setInputQuery] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [expandedSources, setExpandedSources] = useState({});

    const messagesEndRef = useRef(null);
    const textareaRef = useRef(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages, loading]);

    const handleSendMessage = async (queryText) => {
        const query = (queryText || inputQuery).trim();
        if (!query || loading) return;

        setError("");
        const userMsgId = `user-${Date.now()}`;
        const userMsg = {
            id: userMsgId,
            sender: "user",
            text: query,
            timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
        };

        setMessages((prev) => [...prev, userMsg]);
        setInputQuery("");

        // Reset textarea height
        if (textareaRef.current) {
            textareaRef.current.style.height = "auto";
        }

        try {
            setLoading(true);
            const res = await sendChatMessage(query);

            const botMsgId = `assistant-${Date.now()}`;
            const botMsg = {
                id: botMsgId,
                sender: "assistant",
                text: res.answer || "I could not find an answer to your question based on the documents.",
                sources: Array.isArray(res.sources) ? res.sources : [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            };

            setMessages((prev) => [...prev, botMsg]);
        } catch (err) {
            console.error("Chat error:", err);
            const detail = err.response?.data?.detail || err.response?.data?.message;
            const errorText = detail || "An error occurred while querying the AI assistant. Please try again.";
            setError(errorText);

            const errorBotMsg = {
                id: `error-${Date.now()}`,
                sender: "assistant",
                isError: true,
                text: `⚠️ Error: ${errorText}`,
                sources: [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            };
            setMessages((prev) => [...prev, errorBotMsg]);
        } finally {
            setLoading(false);
            if (textareaRef.current) {
                textareaRef.current.focus();
            }
        }
    };

    const handleKeyDown = (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    const handleTextareaInput = (e) => {
        setInputQuery(e.target.value);
        e.target.style.height = "auto";
        e.target.style.height = `${Math.min(e.target.scrollHeight, 120)}px`;
    };

    const handleClearChat = () => {
        setMessages([
            {
                id: "welcome-reset",
                sender: "assistant",
                text: "Chat cleared! What would you like to ask about the uploaded documents?",
                sources: [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            }
        ]);
        setError("");
    };

    const toggleSources = (msgId) => {
        setExpandedSources((prev) => ({
            ...prev,
            [msgId]: !prev[msgId]
        }));
    };

    const displayName = user?.name || (userEmail ? userEmail.split("@")[0] : "You");
    const userInitial = displayName.trim()[0].toUpperCase();

    return (
        <div className="chat-page-container">
            {/* Header */}
            <div className="chat-header">
                <div className="chat-header-info">
                    <div className="chat-header-icon">🤖</div>
                    <div>
                        <h2>ORAS AI Assistant</h2>
                        <span className="chat-header-subtitle">
                            Document Intelligence & Semantic Q&A
                        </span>
                    </div>
                </div>

                <div className="chat-header-actions">
                    <button
                        className="btn-secondary btn-clear-chat"
                        onClick={handleClearChat}
                        title="Clear conversation"
                        id="clear-chat-btn"
                    >
                        <span>🔄</span> Clear Chat
                    </button>
                </div>
            </div>

            {/* Chat Body */}
            <div className="chat-card">
                <div className="chat-messages-scroll">
                    {/* Welcome banner if only 1 message */}
                    {messages.length === 1 && (
                        <div className="chat-suggestions-banner">
                            <p className="suggestions-title">💡 Suggested queries to get started:</p>
                            <div className="suggestions-chips">
                                {SUGGESTED_PROMPTS.map((prompt, index) => (
                                    <button
                                        key={index}
                                        className="suggestion-chip"
                                        onClick={() => handleSendMessage(prompt)}
                                    >
                                        {prompt}
                                    </button>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Messages */}
                    {messages.map((msg) => {
                        const isUser = msg.sender === "user";
                        const showSources = expandedSources[msg.id];

                        return (
                            <div
                                key={msg.id}
                                className={`chat-message-row ${isUser ? "chat-msg-user" : "chat-msg-bot"}`}
                            >
                                <div className="chat-avatar">
                                    {isUser ? userInitial : "🤖"}
                                </div>

                                <div className="chat-bubble-wrapper">
                                    <div className="chat-meta">
                                        <span className="chat-sender-name">
                                            {isUser ? displayName : "AI Assistant"}
                                        </span>
                                        <span className="chat-timestamp">{msg.timestamp}</span>
                                    </div>

                                    <div className={`chat-bubble ${msg.isError ? "chat-bubble-error" : ""}`}>
                                        <div className="chat-text-content">
                                            {msg.text.split("\n").map((line, i) => (
                                                <p key={i} style={{ margin: line ? "0.25rem 0" : "0.5rem 0" }}>
                                                    {line}
                                                </p>
                                            ))}
                                        </div>

                                        {/* Sources cited */}
                                        {msg.sources && msg.sources.length > 0 && (
                                            <div className="chat-sources-section">
                                                <button
                                                    type="button"
                                                    className="chat-sources-toggle"
                                                    onClick={() => toggleSources(msg.id)}
                                                >
                                                    <span>📚 Sources ({msg.sources.length})</span>
                                                    <span>{showSources ? "▲" : "▼"}</span>
                                                </button>

                                                {showSources && (
                                                    <div className="chat-sources-list animate-fade-in">
                                                        {msg.sources.map((src, sIdx) => {
                                                            const srcName = typeof src === "string" ? src : (src.filename || src.source || `Source ${sIdx + 1}`);
                                                            const pageNum = src.page !== undefined ? `Page ${src.page}` : null;
                                                            return (
                                                                <div key={sIdx} className="chat-source-item">
                                                                    <span className="source-icon">📄</span>
                                                                    <span className="source-name">{srcName}</span>
                                                                    {pageNum && (
                                                                        <span className="source-page-badge">{pageNum}</span>
                                                                    )}
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                )}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                        );
                    })}

                    {/* Thinking indicator */}
                    {loading && (
                        <div className="chat-message-row chat-msg-bot">
                            <div className="chat-avatar">🤖</div>
                            <div className="chat-bubble-wrapper">
                                <div className="chat-meta">
                                    <span className="chat-sender-name">AI Assistant</span>
                                </div>
                                <div className="chat-bubble chat-bubble-loading">
                                    <div className="typing-indicator">
                                        <span />
                                        <span />
                                        <span />
                                    </div>
                                    <span className="thinking-text">Searching indexed documents & generating response...</span>
                                </div>
                            </div>
                        </div>
                    )}

                    <div ref={messagesEndRef} />
                </div>

                {/* Input form */}
                <div className="chat-input-container">
                    <form
                        onSubmit={(e) => {
                            e.preventDefault();
                            handleSendMessage();
                        }}
                        className="chat-input-form"
                    >
                        <textarea
                            ref={textareaRef}
                            value={inputQuery}
                            onChange={handleTextareaInput}
                            onKeyDown={handleKeyDown}
                            placeholder="Ask a question about your uploaded documents (Press Enter to send)..."
                            className="chat-textarea"
                            rows={1}
                            disabled={loading}
                            id="chat-input-box"
                        />
                        <button
                            type="submit"
                            className="btn-primary chat-send-btn"
                            disabled={!inputQuery.trim() || loading}
                            id="send-chat-btn"
                            title="Send message"
                        >
                            {loading ? (
                                <span className="spinner-small" />
                            ) : (
                                <span>➤</span>
                            )}
                        </button>
                    </form>
                    <div className="chat-input-footer">
                        <span>AI responses are generated based on currently indexed PDF documents.</span>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Chat;
