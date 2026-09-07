import { useState, useEffect } from "react";
import { getAllComments, createComment, updateComment, deleteComment } from "../api/commentApi";
import { getAllTasks } from "../api/taskApi";
import { getAllUsers } from "../api/userApi";
import { useAuth } from "../context/AuthContext";

const Comments = () => {
    const { userId: currentUserId, user: authUser } = useAuth();
    const [comments, setComments] = useState([]);
    const [tasks, setTasks] = useState([]);
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Filter
    const [selectedTaskId, setSelectedTaskId] = useState("");

    // Form
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingComment, setEditingComment] = useState(null);
    const [content, setContent] = useState("");
    const [taskId, setTaskId] = useState("");
    const [userId, setUserId] = useState("");
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        loadData();
    }, [selectedTaskId]);

    const loadData = async () => {
        try {
            setLoading(true);
            const params = selectedTaskId ? { taskId: selectedTaskId } : {};
            const [commentsData, tasksData, usersData] = await Promise.all([
                getAllComments(params),
                getAllTasks().catch(() => []),
                getAllUsers().catch(() => [])
            ]);
            setComments(commentsData || []);
            setTasks(tasksData || []);
            setUsers(usersData || []);
        } catch (err) {
            console.error(err);
            setError("Failed to load comments.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (comment = null) => {
        setEditingComment(comment);
        if (comment) {
            setContent(comment.content || "");
            setTaskId(comment.taskId || "");
            setUserId(comment.userId || "");
        } else {
            setContent("");
            setTaskId(tasks.length > 0 ? tasks[0].taskId : "");
            setUserId(currentUserId || "");
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingComment(null);
        setContent("");
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSubmitting(true);
        setError("");

        try {
            if (editingComment) {
                await updateComment(editingComment.commentId, { content });
            } else {
                await createComment({
                    content,
                    taskId: parseInt(taskId),
                    userId: parseInt(currentUserId || userId)
                });
            }
            handleCloseModal();
            loadData();
        } catch (err) {
            console.error(err);
            setError("Failed to save comment.");
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Delete this comment?")) return;
        try {
            await deleteComment(id);
            loadData();
        } catch (err) {
            console.error(err);
            setError("Failed to delete comment.");
        }
    };

    const getTaskTitle = (tId) => {
        const t = tasks.find(tsk => tsk.taskId === tId);
        return t ? t.title : `Task #${tId}`;
    };

    const getUserName = (uId) => {
        const u = users.find(usr => usr.userId === uId);
        return u ? u.name : `User #${uId}`;
    };

    return (
        <div className="page-container">
            <div className="page-header-actions">
                <div>
                    <h2>Task Comments</h2>
                    <p>Discuss, leave feedback, and track task conversations.</p>
                </div>
                <button className="btn-primary" onClick={() => handleOpenModal()}>
                    + Add Comment
                </button>
            </div>

            {error && <div className="error-alert">{error}</div>}

            <div className="filter-toolbar">
                <select
                    value={selectedTaskId}
                    onChange={(e) => setSelectedTaskId(e.target.value)}
                    className="filter-select"
                >
                    <option value="">All Tasks</option>
                    {tasks.map(t => (
                        <option key={t.taskId} value={t.taskId}>{t.title}</option>
                    ))}
                </select>
            </div>

            {loading ? (
                <div className="page-loader">Loading comments...</div>
            ) : comments.length === 0 ? (
                <div className="empty-state">No comments found.</div>
            ) : (
                <div className="comments-feed">
                    {comments.map((c) => (
                        <div key={c.commentId} className="comment-card">
                            <div className="comment-header">
                                <div className="comment-user">
                                    <span className="comment-avatar">👤</span>
                                    <div>
                                        <div className="comment-author-name">{c.userName || getUserName(c.userId)}</div>
                                        <div className="comment-task-tag">On: <strong>{c.taskTitle || getTaskTitle(c.taskId)}</strong></div>
                                    </div>
                                </div>
                                <div className="comment-date">
                                    {c.createdAt ? new Date(c.createdAt).toLocaleString() : "Just now"}
                                </div>
                            </div>
                            <div className="comment-body">
                                {c.content}
                            </div>
                            <div className="comment-actions">
                                <button className="btn-edit-sm" onClick={() => handleOpenModal(c)}>
                                    Edit
                                </button>
                                <button className="btn-delete-sm" onClick={() => handleDelete(c.commentId)}>
                                    Delete
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {isModalOpen && (
                <div className="modal-backdrop">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>{editingComment ? "Edit Comment" : "Add Comment"}</h3>
                            <button className="close-btn" onClick={handleCloseModal}>&times;</button>
                        </div>
                        <form onSubmit={handleSubmit}>
                            {!editingComment && (
                                <>
                                    <div className="form-group">
                                        <label>Select Task</label>
                                        {tasks.length > 0 ? (
                                            <select
                                                value={taskId}
                                                onChange={(e) => setTaskId(e.target.value)}
                                                required
                                            >
                                                {tasks.map(t => (
                                                    <option key={t.taskId} value={t.taskId}>{t.title}</option>
                                                ))}
                                            </select>
                                        ) : (
                                            <input
                                                type="number"
                                                placeholder="Task ID"
                                                value={taskId}
                                                onChange={(e) => setTaskId(e.target.value)}
                                                required
                                            />
                                        )}
                                    </div>

                                    <div className="form-group">
                                        <label>Author</label>
                                        <input
                                            type="text"
                                            value={authUser?.name ? `${authUser.name} (${authUser.email || "You"})` : (currentUserId ? `User #${currentUserId}` : "Current User")}
                                            disabled
                                            readOnly
                                            style={{ backgroundColor: "var(--color-bg-secondary, #f1f5f9)", cursor: "not-allowed" }}
                                        />
                                    </div>
                                </>
                            )}

                            <div className="form-group">
                                <label>Comment</label>
                                <textarea
                                    rows="4"
                                    placeholder="Write your comment here..."
                                    value={content}
                                    onChange={(e) => setContent(e.target.value)}
                                    maxLength={500}
                                    required
                                />
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={handleCloseModal}>
                                    Cancel
                                </button>
                                <button type="submit" className="btn-primary" disabled={submitting}>
                                    {submitting ? "Saving..." : (editingComment ? "Update" : "Post Comment")}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Comments;
