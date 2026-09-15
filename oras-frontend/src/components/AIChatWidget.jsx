import { useState, useRef, useEffect, useCallback } from "react";
import { sendChatMessage, clearChatSession } from "../api/ragApi";
import { useAuth } from "../context/AuthContext";

const generateSessionId = () =>
    typeof crypto !== "undefined" && crypto.randomUUID
        ? crypto.randomUUID()
        : `sess-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;

const AIChatWidget = () => {
    const { user, userEmail } = useAuth();
    const [isOpen, setIsOpen] = useState(false);
    const [sessionId, setSessionId] = useState(generateSessionId);
    const [messages, setMessages] = useState([
        {
            id: "welcome-1",
            sender: "assistant",
            text: "Welcome! How can I help you?",
            sources: [],
            timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
        }
    ]);
    const [inputQuery, setInputQuery] = useState("");
    const [loading, setLoading] = useState(false);
    const [expandedSources, setExpandedSources] = useState({});
    const [hasUnread, setHasUnread] = useState(false);

    const widgetRef = useRef(null);
    const messagesEndRef = useRef(null);
    const inputRef = useRef(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    const closeWidgetAndClearMemory = useCallback(() => {
        if (sessionId) {
            clearChatSession(sessionId);
        }
        setIsOpen(false);
        setSessionId(generateSessionId());
        setMessages([
            {
                id: `welcome-${Date.now()}`,
                sender: "assistant",
                text: "Welcome! How can I help you?",
                sources: [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            }
        ]);
        setExpandedSources({});
        setInputQuery("");
        setHasUnread(false);
    }, [sessionId]);

    useEffect(() => {
        if (isOpen) {
            scrollToBottom();
            setHasUnread(false);
            setTimeout(() => {
                inputRef.current?.focus();
            }, 100);
        }
    }, [isOpen, messages, loading]);

    // Handle outside click & escape key
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (widgetRef.current && !widgetRef.current.contains(e.target)) {
                closeWidgetAndClearMemory();
            }
        };
        const handleEscapeKey = (e) => {
            if (e.key === "Escape") {
                closeWidgetAndClearMemory();
            }
        };

        if (isOpen) {
            document.addEventListener("mousedown", handleClickOutside);
            document.addEventListener("keydown", handleEscapeKey);
        }
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
            document.removeEventListener("keydown", handleEscapeKey);
        };
    }, [isOpen, closeWidgetAndClearMemory]);

    const handleSendMessage = async (e) => {
        if (e) e.preventDefault();
        const query = inputQuery.trim();
        if (!query || loading) return;

        const userMsg = {
            id: `user-${Date.now()}`,
            sender: "user",
            text: query,
            timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
        };

        setMessages((prev) => [...prev, userMsg]);
        setInputQuery("");

        try {
            setLoading(true);
            const res = await sendChatMessage(query, sessionId);

            const botMsg = {
                id: `assistant-${Date.now()}`,
                sender: "assistant",
                text: res.answer || "I could not find an answer to your question based on the documents.",
                sources: Array.isArray(res.sources) ? res.sources : [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            };

            setMessages((prev) => [...prev, botMsg]);
            if (!isOpen) {
                setHasUnread(true);
            }
        } catch (err) {
            console.error("Widget chat error:", err);
            const detail = err.response?.data?.detail || err.response?.data?.message;
            const errorText = detail || "An error occurred while querying the AI assistant. Please try again.";

            const errorBotMsg = {
                id: `error-${Date.now()}`,
                sender: "assistant",
                isError: true,
                text: `⚠️ ${errorText}`,
                sources: [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            };
            setMessages((prev) => [...prev, errorBotMsg]);
        } finally {
            setLoading(false);
        }
    };

    const handleKeyDown = (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    const handleClearChat = () => {
        if (sessionId) {
            clearChatSession(sessionId);
        }
        setSessionId(generateSessionId());
        setMessages([
            {
                id: `welcome-${Date.now()}`,
                sender: "assistant",
                text: "Welcome! How can I help you?",
                sources: [],
                timestamp: new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
            }
        ]);
        setExpandedSources({});
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
        <div className="ai-chat-widget-wrapper" ref={widgetRef}>
            {/* Chat Popup Panel */}
            {isOpen && (
                <div className="ai-chat-panel animate-fade-in" id="ai-chat-popup-window">
                    {/* Header */}
                    <div className="ai-chat-panel-header">
                        <div className="header-info">
                            <div className="header-avatar">
                                <img src="/ai_icon_ss.png" alt="AI Icon" className="header-ai-icon-img" />
                            </div>
                            <div>
                                <h3>ORAS Assistant</h3>
                                <span className="header-status">
                                    <span className="status-dot"></span> Knowledge Base Connected
                                </span>
                            </div>
                        </div>
                        <div className="header-actions">
                            <button
                                className="widget-header-btn"
                                onClick={handleClearChat}
                                title="Clear Chat"
                                id="clear-widget-chat-btn"
                            >
                                🔄
                            </button>
                            <button
                                className="widget-header-btn close-btn"
                                onClick={closeWidgetAndClearMemory}
                                title="Close Chat"
                                id="close-widget-chat-btn"
                            >
                                ✖
                            </button>
                        </div>
                    </div>

                    {/* Body */}
                    <div className="ai-chat-panel-body">
                        {messages.map((msg) => {
                            const isUser = msg.sender === "user";
                            const showSources = expandedSources[msg.id];

                            return (
                                <div
                                    key={msg.id}
                                    className={`widget-msg-row ${isUser ? "user-row" : "bot-row"}`}
                                >
                                    <div className="widget-avatar">
                                        {isUser ? userInitial : <img src="/ai_icon_ss.png" alt="AI" className="widget-ai-icon-img" />}
                                    </div>
                                    <div className="widget-bubble-wrapper">
                                        <div className={`widget-bubble ${msg.isError ? "error-bubble" : ""}`}>
                                            <div className="widget-text-content">
                                                {msg.text.split("\n").map((line, i) => (
                                                    <p key={i} style={{ margin: line ? "0.2rem 0" : "0.4rem 0" }}>
                                                        {line}
                                                    </p>
                                                ))}
                                            </div>

                                            {/* Sources */}
                                            {msg.sources && msg.sources.length > 0 && (
                                                <div className="widget-sources-section">
                                                    <button
                                                        type="button"
                                                        className="widget-sources-toggle"
                                                        onClick={() => toggleSources(msg.id)}
                                                    >
                                                        <span>📚 Sources ({msg.sources.length})</span>
                                                        <span>{showSources ? "▲" : "▼"}</span>
                                                    </button>

                                                    {showSources && (
                                                        <div className="widget-sources-list">
                                                            {msg.sources.map((src, sIdx) => {
                                                                const isWeb = src.type === "web" || (!src.document_id && (src.url || src.title));
                                                                if (isWeb) {
                                                                    return (
                                                                        <a
                                                                            key={sIdx}
                                                                            href={src.url}
                                                                            target="_blank"
                                                                            rel="noopener noreferrer"
                                                                            className="widget-source-item widget-source-link"
                                                                        >
                                                                            🌐 {src.title || src.url || `Web Result ${sIdx + 1}`}
                                                                        </a>
                                                                    );
                                                                }

                                                                const srcName = typeof src === "string" ? src : (src.filename || src.source || `Source ${sIdx + 1}`);
                                                                const pageNum = src.page !== undefined ? ` (p.${src.page})` : "";
                                                                return (
                                                                    <div key={sIdx} className="widget-source-item">
                                                                        📄 {srcName}{pageNum}
                                                                    </div>
                                                                );
                                                            })}
                                                        </div>
                                                    )}
                                                </div>
                                            )}
                                        </div>
                                        <span className="widget-timestamp">{msg.timestamp}</span>
                                    </div>
                                </div>
                            );
                        })}

                        {loading && (
                            <div className="widget-msg-row bot-row">
                                <div className="widget-avatar">
                                    <img src="/ai_icon_ss.png" alt="AI" className="widget-ai-icon-img" />
                                </div>
                                <div className="widget-bubble-wrapper">
                                    <div className="widget-bubble loading-bubble">
                                        <div className="typing-dots">
                                            <span></span>
                                            <span></span>
                                            <span></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}
                        <div ref={messagesEndRef} />
                    </div>

                    {/* Footer / Input */}
                    <div className="ai-chat-panel-footer">
                        <form onSubmit={handleSendMessage} className="widget-input-form">
                            <input
                                ref={inputRef}
                                type="text"
                                value={inputQuery}
                                onChange={(e) => setInputQuery(e.target.value)}
                                onKeyDown={handleKeyDown}
                                placeholder="Type a message..."
                                className="widget-input"
                                disabled={loading}
                                id="ai-chat-widget-input-box"
                            />
                            <button
                                type="submit"
                                className="widget-send-btn"
                                disabled={!inputQuery.trim() || loading}
                                id="ai-chat-widget-send-btn"
                                title="Send"
                            >
                                ➤
                            </button>
                        </form>
                    </div>
                </div>
            )}

            {/* Floating Action Button (FAB) */}
            <button
                className={`ai-chat-fab ${isOpen ? "active" : ""}`}
                onClick={() => {
                    if (isOpen) {
                        closeWidgetAndClearMemory();
                    } else {
                        setIsOpen(true);
                    }
                }}
                aria-label="Toggle AI Chat"
                id="ai-chat-fab-button"
                title="AI Knowledge Assistant"
            >
                {isOpen ? (
                    <span className="fab-icon close-icon">✖</span>
                ) : (
                    <span className="fab-icon chat-icon">
                        <img src="/ai_icon_ss.png" alt="AI Assistant" className="fab-ai-icon-img" />
                    </span>
                )}
                {hasUnread && !isOpen && <span className="fab-unread-dot"></span>}
            </button>
        </div>
    );
};

export default AIChatWidget;
