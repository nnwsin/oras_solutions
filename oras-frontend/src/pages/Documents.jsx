import { useState, useEffect, useRef } from "react";
import { getAllDocuments, uploadDocument, deleteDocument } from "../api/ragApi";
import { useAuth } from "../context/AuthContext";

const Documents = () => {
    const { userRole } = useAuth();
    const isAdmin = userRole === "Admin" || userRole === 3;

    const [documents, setDocuments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [uploading, setUploading] = useState(false);
    const [deletingId, setDeletingId] = useState(null);
    const [searchQuery, setSearchQuery] = useState("");
    const [error, setError] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [selectedFile, setSelectedFile] = useState(null);
    const [isDragOver, setIsDragOver] = useState(false);

    const fileInputRef = useRef(null);

    useEffect(() => {
        loadDocuments();
    }, []);

    const loadDocuments = async () => {
        try {
            setLoading(true);
            setError("");
            const docs = await getAllDocuments();
            setDocuments(Array.isArray(docs) ? docs : []);
        } catch (err) {
            console.error(err);
            if (err.response?.status === 403) {
                setError("Access denied. Only Administrators can view or manage documents.");
            } else {
                setError(err.response?.data?.detail || "Failed to load documents.");
            }
        } finally {
            setLoading(false);
        }
    };

    const handleFileSelect = (file) => {
        setError("");
        setSuccessMessage("");
        if (!file) return;

        if (!file.name.toLowerCase().endsWith(".pdf")) {
            setError("Only PDF files (.pdf) are supported.");
            setSelectedFile(null);
            return;
        }

        setSelectedFile(file);
    };

    const handleFileInputChange = (e) => {
        const file = e.target.files?.[0];
        handleFileSelect(file);
    };

    const handleDragOver = (e) => {
        e.preventDefault();
        setIsDragOver(true);
    };

    const handleDragLeave = (e) => {
        e.preventDefault();
        setIsDragOver(false);
    };

    const handleDrop = (e) => {
        e.preventDefault();
        setIsDragOver(false);
        const file = e.dataTransfer.files?.[0];
        handleFileSelect(file);
    };

    const handleUpload = async (e) => {
        e.preventDefault();
        if (!selectedFile) {
            setError("Please select a PDF document to upload.");
            return;
        }

        try {
            setUploading(true);
            setError("");
            setSuccessMessage("");

            const res = await uploadDocument(selectedFile);
            setSuccessMessage(
                `"${res.filename}" uploaded & indexed successfully! (${res.pages || 0} pages, ${res.chunks || 0} chunks)`
            );
            setSelectedFile(null);
            if (fileInputRef.current) {
                fileInputRef.current.value = "";
            }
            await loadDocuments();
        } catch (err) {
            console.error(err);
            const detail = err.response?.data?.detail;
            if (typeof detail === "string") {
                setError(detail);
            } else if (err.response?.status === 403) {
                setError("Forbidden: You must have an Admin role to upload documents.");
            } else {
                setError(err.response?.data?.message || "Failed to upload and index document.");
            }
        } finally {
            setUploading(false);
        }
    };

    const handleDelete = async (docId, filename) => {
        if (!window.confirm(`Are you sure you want to delete "${filename}"? This will remove its indexed embeddings.`)) {
            return;
        }

        try {
            setDeletingId(docId);
            setError("");
            setSuccessMessage("");

            await deleteDocument(docId);
            setSuccessMessage(`Document "${filename}" deleted successfully.`);
            setDocuments((prev) => prev.filter((d) => d.document_id !== docId));
        } catch (err) {
            console.error(err);
            if (err.response?.status === 403) {
                setError("Forbidden: You must have an Admin role to delete documents.");
            } else {
                setError(err.response?.data?.detail || "Failed to delete document.");
            }
        } finally {
            setDeletingId(null);
        }
    };

    const formatFileSize = (bytes) => {
        if (!bytes) return "0 B";
        const k = 1024;
        const sizes = ["B", "KB", "MB", "GB"];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + " " + sizes[i];
    };

    const filteredDocs = documents.filter((doc) =>
        (doc.filename || "").toLowerCase().includes(searchQuery.toLowerCase())
    );

    if (!isAdmin) {
        return (
            <div className="empty-state-card" style={{ padding: "48px 24px", textAlign: "center" }}>
                <div style={{ fontSize: "3rem", marginBottom: "16px" }}>🔒</div>
                <h2>Admin Access Required</h2>
                <p style={{ color: "var(--text-muted)", marginTop: "8px" }}>
                    Only users with Administrator privileges can upload and manage RAG documents.
                </p>
            </div>
        );
    }

    return (
        <div className="documents-page-container">
            {/* Header */}
            <div className="page-header">
                <div>
                    <h2>Document Management</h2>
                    <p className="page-header-subtitle">
                        Upload PDF documents for AI retrieval-augmented generation (RAG) and semantic search.
                    </p>
                </div>
                <div className="page-header-badge">
                    <span className="doc-count-pill">{documents.length} Indexed Documents</span>
                </div>
            </div>

            {/* Notifications */}
            {error && (
                <div className="error-alert animate-fade-in" style={{ marginBottom: "1.5rem" }}>
                    <span>⚠️ {error}</span>
                    <button className="alert-close-btn" onClick={() => setError("")}>×</button>
                </div>
            )}
            {successMessage && (
                <div className="success-alert animate-fade-in" style={{ marginBottom: "1.5rem" }}>
                    <span>✅ {successMessage}</span>
                    <button className="alert-close-btn" onClick={() => setSuccessMessage("")}>×</button>
                </div>
            )}

            {/* Upload Section */}
            <div className="dashboard-card doc-upload-card" style={{ marginBottom: "2rem" }}>
                <div className="card-header">
                    <h3>Upload New Document</h3>
                    <span className="badge badge-info">PDF Only</span>
                </div>
                <form onSubmit={handleUpload} className="doc-upload-form">
                    <div
                        className={`doc-dropzone ${isDragOver ? "drag-active" : ""} ${selectedFile ? "has-file" : ""}`}
                        onDragOver={handleDragOver}
                        onDragLeave={handleDragLeave}
                        onDrop={handleDrop}
                        onClick={() => fileInputRef.current?.click()}
                    >
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept=".pdf,application/pdf"
                            onChange={handleFileInputChange}
                            style={{ display: "none" }}
                            id="pdf-file-input"
                        />
                        <div className="dropzone-icon">
                            {selectedFile ? "📄" : "☁️"}
                        </div>
                        {selectedFile ? (
                            <div className="dropzone-file-info">
                                <span className="dropzone-filename">{selectedFile.name}</span>
                                <span className="dropzone-filesize">
                                    {formatFileSize(selectedFile.size)}
                                </span>
                                <span className="dropzone-change-hint">Click or drag another file to replace</span>
                            </div>
                        ) : (
                            <div className="dropzone-prompt">
                                <strong>Click to choose a file</strong> or drag & drop a PDF here
                                <span className="dropzone-hint">Indexed chunks and vectors are auto-generated</span>
                            </div>
                        )}
                    </div>

                    <div className="doc-upload-actions">
                        {selectedFile && (
                            <button
                                type="button"
                                className="btn-secondary"
                                onClick={() => {
                                    setSelectedFile(null);
                                    if (fileInputRef.current) fileInputRef.current.value = "";
                                }}
                                disabled={uploading}
                            >
                                Clear
                            </button>
                        )}
                        <button
                            type="submit"
                            className="btn-primary doc-upload-btn"
                            disabled={!selectedFile || uploading}
                            id="upload-document-btn"
                        >
                            {uploading ? (
                                <>
                                    <span className="spinner-small" />
                                    Indexing Document...
                                </>
                            ) : (
                                <>
                                    <span>📤</span>
                                    Upload & Process
                                </>
                            )}
                        </button>
                    </div>
                </form>
            </div>

            {/* Document List Section */}
            <div className="dashboard-card">
                <div className="card-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
                    <h3>Indexed Documents ({documents.length})</h3>
                    <div className="doc-search-wrapper">
                        <input
                            type="text"
                            className="search-input"
                            placeholder="Search documents by name..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            id="search-documents-input"
                        />
                    </div>
                </div>

                {loading ? (
                    <div style={{ textAlign: "center", padding: "40px", color: "var(--text-muted)" }}>
                        <div className="spinner-small" style={{ margin: "0 auto 12px" }} />
                        Loading documents...
                    </div>
                ) : filteredDocs.length === 0 ? (
                    <div className="empty-state-card" style={{ padding: "40px 20px", textAlign: "center" }}>
                        <div style={{ fontSize: "2.5rem", marginBottom: "12px" }}>📁</div>
                        <h4>{searchQuery ? "No matching documents found" : "No documents indexed yet"}</h4>
                        <p style={{ color: "var(--text-muted)", fontSize: "0.9rem", marginTop: "6px" }}>
                            {searchQuery
                                ? "Try adjusting your search query."
                                : "Upload a PDF above to enable AI question answering over your documents."}
                        </p>
                    </div>
                ) : (
                    <div className="table-responsive">
                        <table className="data-table">
                            <thead>
                                <tr>
                                    <th>Document</th>
                                    <th>Document ID</th>
                                    <th>Status</th>
                                    <th style={{ textAlign: "right" }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredDocs.map((doc) => {
                                    const isDeleting = deletingId === doc.document_id;
                                    return (
                                        <tr key={doc.document_id}>
                                            <td>
                                                <div className="doc-name-cell">
                                                    <span className="doc-icon">📕</span>
                                                    <span className="doc-title" title={doc.filename}>
                                                        {doc.filename}
                                                    </span>
                                                </div>
                                            </td>
                                            <td>
                                                <code className="doc-id-pill" title={doc.document_id}>
                                                    {doc.document_id.length > 12
                                                        ? `${doc.document_id.substring(0, 8)}...${doc.document_id.slice(-4)}`
                                                        : doc.document_id}
                                                </code>
                                            </td>
                                            <td>
                                                <span className="badge badge-success">
                                                    ● Indexed
                                                </span>
                                            </td>
                                            <td style={{ textAlign: "right" }}>
                                                <button
                                                    className="btn-delete-sm"
                                                    onClick={() => handleDelete(doc.document_id, doc.filename)}
                                                    disabled={isDeleting}
                                                    title="Delete document and embeddings"
                                                    id={`delete-doc-${doc.document_id}`}
                                                >
                                                    {isDeleting ? "Deleting..." : "🗑️ Delete"}
                                                </button>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Documents;
