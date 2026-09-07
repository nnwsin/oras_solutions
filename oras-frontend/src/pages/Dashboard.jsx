import { useState, useEffect } from "react";
import { getAllProjects } from "../api/projectApi";
import { getAllTasks } from "../api/taskApi";
import { getAllComments } from "../api/commentApi";
import { getAllUsers } from "../api/userApi";
import { Link } from "react-router-dom";

const Dashboard = () => {
    const [counts, setCounts] = useState({
        projects: 0,
        tasks: 0,
        pendingTasks: 0,
        completedTasks: 0,
        comments: 0,
        users: 0
    });
    const [recentTasks, setRecentTasks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                setLoading(true);
                const [projects, tasks, comments, users] = await Promise.all([
                    getAllProjects().catch(() => []),
                    getAllTasks().catch(() => []),
                    getAllComments().catch(() => []),
                    getAllUsers().catch(() => [])
                ]);

                const pending = Array.isArray(tasks) ? tasks.filter(t => t.status === 1 || t.status === "Pending").length : 0;
                const completed = Array.isArray(tasks) ? tasks.filter(t => t.status === 3 || t.status === "Completed").length : 0;

                setCounts({
                    projects: Array.isArray(projects) ? projects.length : 0,
                    tasks: Array.isArray(tasks) ? tasks.length : 0,
                    pendingTasks: pending,
                    completedTasks: completed,
                    comments: Array.isArray(comments) ? comments.length : 0,
                    users: Array.isArray(users) ? users.length : 0
                });

                const sortedTasks = Array.isArray(tasks)
                    ? [...tasks].sort((a, b) => {
                        if (a.createdAt && b.createdAt) {
                            return new Date(b.createdAt) - new Date(a.createdAt);
                        }
                        return (b.taskId || 0) - (a.taskId || 0);
                    }).slice(0, 5)
                    : [];

                setRecentTasks(sortedTasks);
            } catch (err) {
                console.error("Failed to load dashboard data", err);
                setError("Failed to load some dashboard data. Check backend connection.");
            } finally {
                setLoading(false);
            }
        };

        fetchDashboardData();
    }, []);

    const getStatusBadgeClass = (status) => {
        switch (status) {
            case 1:
            case "Pending":
                return "badge-pending";
            case 2:
            case "InProgress":
                return "badge-progress";
            case 3:
            case "Completed":
                return "badge-completed";
            case 4:
            case "Cancelled":
                return "badge-cancelled";
            default:
                return "badge-default";
        }
    };

    const getStatusText = (status) => {
        if (status === 1 || status === "Pending") return "Pending";
        if (status === 2 || status === "InProgress") return "In Progress";
        if (status === 3 || status === "Completed") return "Completed";
        if (status === 4 || status === "Cancelled") return "Cancelled";
        return status;
    };

    if (loading) {
        return <div className="page-loader">Loading dashboard metrics...</div>;
    }

    return (
        <div className="dashboard-page">
            <div className="page-header">
                <h2>Dashboard Overview</h2>
                <p>Welcome back! Here's a snapshot of your workspace.</p>
            </div>

            {error && <div className="error-alert">{error}</div>}

            <div className="overview-cards">
                <div className="overview-card card-projects">
                    <div className="card-icon">📁</div>
                    <div className="card-info">
                        <h3>Total Projects</h3>
                        <p className="card-number">{counts.projects}</p>
                    </div>
                    <Link to="/projects" className="card-link">View Projects &rarr;</Link>
                </div>

                <div className="overview-card card-tasks">
                    <div className="card-icon">📋</div>
                    <div className="card-info">
                        <h3>Total Tasks</h3>
                        <p className="card-number">{counts.tasks}</p>
                        <span className="card-subtext">{counts.pendingTasks} Pending · {counts.completedTasks} Done</span>
                    </div>
                    <Link to="/tasks" className="card-link">Manage Tasks &rarr;</Link>
                </div>

                <div className="overview-card card-comments">
                    <div className="card-icon">💬</div>
                    <div className="card-info">
                        <h3>Comments</h3>
                        <p className="card-number">{counts.comments}</p>
                    </div>
                    <Link to="/comments" className="card-link">View Comments &rarr;</Link>
                </div>

                <div className="overview-card card-users">
                    <div className="card-icon">👥</div>
                    <div className="card-info">
                        <h3>Team Members</h3>
                        <p className="card-number">{counts.users}</p>
                    </div>
                    <Link to="/users" className="card-link">Manage Team &rarr;</Link>
                </div>
            </div>

            <div className="dashboard-section mt-6">
                <div className="section-header">
                    <h3>Recent Tasks</h3>
                    <Link to="/tasks" className="btn-secondary-sm">View All</Link>
                </div>

                {recentTasks.length === 0 ? (
                    <div className="empty-state">No tasks created yet.</div>
                ) : (
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Priority</th>
                                <th>Status</th>
                                <th>Due Date</th>
                            </tr>
                        </thead>
                        <tbody>
                            {recentTasks.map((t) => (
                                <tr key={t.taskId}>
                                    <td className="font-semibold">{t.title}</td>
                                    <td><span className={`priority-pill priority-${(t.priority || "Low").toLowerCase()}`}>{t.priority}</span></td>
                                    <td>
                                        <span className={`status-badge ${getStatusBadgeClass(t.status)}`}>
                                            {getStatusText(t.status)}
                                        </span>
                                    </td>
                                    <td>{t.dueDate ? new Date(t.dueDate).toLocaleDateString() : "N/A"}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
};

export default Dashboard;