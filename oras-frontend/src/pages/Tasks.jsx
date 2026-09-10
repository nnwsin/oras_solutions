import { useState, useEffect } from "react";
import { getAllTasks, createTask, updateTask, deleteTask } from "../api/taskApi";
import { getAllProjects } from "../api/projectApi";
import { getAllUsers } from "../api/userApi";
import { useAuth } from "../context/AuthContext";
import StatusDropdown from "../components/StatusDropdown";

const Tasks = () => {
    const { userId: currentUserId, userRole, user: authUser } = useAuth();
    const isAdmin = userRole === "Admin" || userRole === 3;
    const isManager = userRole === "Manager" || userRole === 2;

    const isEmployee = userRole === "Employee" || userRole === 1;
    const canCreateOrEdit = isAdmin || isManager;

    const [tasks, setTasks] = useState([]);
    const [projects, setProjects] = useState([]);
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Filters
    const [filterProject, setFilterProject] = useState("");
    const [filterStatus, setFilterStatus] = useState("");
    const [filterAssignee, setFilterAssignee] = useState("");
    const [searchQuery, setSearchQuery] = useState("");

    // Modal State
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingTask, setEditingTask] = useState(null);

    // Form fields
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [status, setStatus] = useState(1);
    const [dueDate, setDueDate] = useState("");
    const [priority, setPriority] = useState("Medium");
    const [projectId, setProjectId] = useState("");
    const [assigneeId, setAssigneeId] = useState("");
    const [submitting, setSubmitting] = useState(false);

    // User filtering lists by role
    const managerUsers = users.filter(u => u.role === "Manager" || u.role === 2);
    const employeeUsers = users.filter(u => u.role === "Employee" || u.role === 1);

    useEffect(() => {
        loadData();
    }, [filterProject, filterStatus, filterAssignee]);

    const loadData = async () => {
        try {
            setLoading(true);
            const params = {};
            if (filterProject) params.projectId = filterProject;
            if (filterStatus) params.status = filterStatus;

            // If user is Manager or Employee, only show tasks assigned to them
            if ((isManager || isEmployee) && currentUserId) {
                params.assigneeId = currentUserId;
            } else if (filterAssignee) {
                params.assigneeId = filterAssignee;
            }

            const [tasksData, projectsData, usersData] = await Promise.all([
                getAllTasks(params),
                getAllProjects().catch(() => []),
                getAllUsers().catch(() => [])
            ]);

            // Ensure client-side filtering as backup for Manager / Employee role
            let processedTasks = [...(tasksData || [])];
            if ((isManager || isEmployee) && currentUserId) {
                processedTasks = processedTasks.filter(t => t.assigneeId === parseInt(currentUserId));
            }

            processedTasks.sort((a, b) => {
                const timeA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
                const timeB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
                if (timeB !== timeA) return timeB - timeA;
                return (b.taskId || 0) - (a.taskId || 0);
            });

            setTasks(processedTasks);
            setProjects(projectsData || []);
            setUsers(usersData || []);
        } catch (err) {
            console.error(err);
            setError("Failed to load tasks.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (task = null) => {
        setEditingTask(task);

        // Projects available to user (Manager only sees projects where they are Owner)
        const userProjects = isManager
            ? projects.filter(p => p.ownerId === parseInt(currentUserId))
            : projects;

        // Current Employees list
        const emps = users.filter(u => u.role === "Employee" || u.role === 1);
        const mgrs = users.filter(u => u.role === "Manager" || u.role === 2);

        if (task) {
            setTitle(task.title || "");
            setDescription(task.description || "");
            setStatus(task.status || 1);
            setDueDate(task.dueDate ? new Date(task.dueDate).toISOString().slice(0, 16) : "");
            setPriority(task.priority || "Medium");
            setProjectId(task.projectId || "");
            setAssigneeId(task.assigneeId || "");
        } else {
            setTitle("");
            setDescription("");
            setStatus(1);
            setDueDate("");
            setPriority("Medium");
            setProjectId(userProjects.length > 0 ? userProjects[0].projectId : "");

            // Default Assignee based on role constraints
            if (isAdmin) {
                setAssigneeId(mgrs.length > 0 ? mgrs[0].userId : "");
            } else if (isManager) {
                setAssigneeId(emps.length > 0 ? emps[0].userId : "");
            } else {
                setAssigneeId("");
            }
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingTask(null);
        setTitle("");
        setDescription("");
        setStatus(1);
        setDueDate("");
        setPriority("Medium");
        setProjectId("");
        setAssigneeId("");
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSubmitting(true);
        setError("");

        try {
            let finalAssigneeId = parseInt(assigneeId);
            if (!editingTask) {
                if (isAdmin) {
                    if (managerUsers.length === 0) {
                        throw new Error("Cannot create task: No user with Manager role found.");
                    }
                    finalAssigneeId = parseInt(assigneeId) || managerUsers[0].userId;
                } else if (isManager) {
                    if (employeeUsers.length === 0) {
                        throw new Error("Cannot create task: No user with Employee role found.");
                    }
                    finalAssigneeId = parseInt(assigneeId) || employeeUsers[0].userId;
                }
            }

            const payload = {
                title,
                description,
                status: parseInt(status),
                dueDate: new Date(dueDate).toISOString(),
                priority,
                projectId: parseInt(projectId),
                assigneeId: finalAssigneeId
            };

            if (editingTask) {
                await updateTask(editingTask.taskId, payload);
            } else {
                const newTask = await createTask(payload);
                if (newTask && newTask.taskId) {
                    setTasks(prev => [newTask, ...prev.filter(t => t.taskId !== newTask.taskId)]);
                }
            }

            handleCloseModal();
            loadData();
        } catch (err) {
            console.error(err);
            setError(err.response?.data?.message || err.response?.data?.Message || err.message || "Failed to save task. Check inputs.");
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this task?")) return;
        try {
            await deleteTask(id);
            loadData();
        } catch (err) {
            console.error(err);
            setError("Failed to delete task.");
        }
    };

    const handleQuickStatusChange = async (task, newStatus) => {
        const updatedStatus = parseInt(newStatus);
        const previousStatus = task.status;

        // Optimistically update local task state immediately without reload/flash
        setTasks(prev => prev.map(t =>
            t.taskId === task.taskId ? { ...t, status: updatedStatus } : t
        ));

        try {
            const payload = {
                title: task.title,
                description: task.description,
                status: updatedStatus,
                dueDate: task.dueDate,
                priority: task.priority,
                projectId: task.projectId,
                assigneeId: task.assigneeId
            };
            await updateTask(task.taskId, payload);
        } catch (err) {
            console.error("Failed to update status", err);
            // Revert state if update failed
            setTasks(prev => prev.map(t =>
                t.taskId === task.taskId ? { ...t, status: previousStatus } : t
            ));
        }
    };

    const getStatusText = (statusVal) => {
        switch (statusVal) {
            case 1:
            case "Pending":
                return "Pending";
            case 2:
            case "InProgress":
                return "In Progress";
            case 3:
            case "Completed":
                return "Completed";
            case 4:
            case "Cancelled":
                return "Cancelled";
            default:
                return statusVal;
        }
    };

    const getStatusBadgeClass = (statusVal) => {
        switch (statusVal) {
            case 1:
            case "1":
            case "Pending":
                return "badge-pending";
            case 2:
            case "2":
            case "InProgress":
                return "badge-progress";
            case 3:
            case "3":
            case "Completed":
                return "badge-completed";
            case 4:
            case "4":
            case "Cancelled":
                return "badge-cancelled";
            default:
                return "";
        }
    };

    const filteredTasks = tasks.filter(t =>
        t.title?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        t.description?.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const getProjectName = (pId) => {
        const proj = projects.find(p => p.projectId === pId);
        return proj ? proj.projectName : `Project #${pId}`;
    };

    const getAssigneeName = (aId) => {
        const u = users.find(usr => usr.userId === aId);
        return u ? u.name : `User #${aId}`;
    };

    return (
        <div className="page-container">
            <div className="page-header-actions">
                <div>
                    <h2>Tasks</h2>
                    <p>Track, assign, and organize tasks across your projects.</p>
                </div>
                {canCreateOrEdit && (
                    <button className="btn-primary" onClick={() => handleOpenModal()}>
                        + New Task
                    </button>
                )}
            </div>

            {error && <div className="error-alert">{error}</div>}

            <div className="filter-toolbar">
                <input
                    type="text"
                    className="search-input"
                    placeholder="Search tasks..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                />

                <select
                    value={filterProject}
                    onChange={(e) => setFilterProject(e.target.value)}
                    className="filter-select"
                >
                    <option value="">All Projects</option>
                    {projects.map(p => (
                        <option key={p.projectId} value={p.projectId}>{p.projectName}</option>
                    ))}
                </select>

                <select
                    value={filterStatus}
                    onChange={(e) => setFilterStatus(e.target.value)}
                    className={`filter-select ${filterStatus ? `status-select ${getStatusBadgeClass(filterStatus)}` : ""}`}
                >
                    <option value="">All Statuses</option>
                    <option value="1" className="status-opt-pending">Pending</option>
                    <option value="2" className="status-opt-progress">In Progress</option>
                    <option value="3" className="status-opt-completed">Completed</option>
                    <option value="4" className="status-opt-cancelled">Cancelled</option>
                </select>

                <select
                    value={filterAssignee}
                    onChange={(e) => setFilterAssignee(e.target.value)}
                    className="filter-select"
                >
                    <option value="">All Assignees</option>
                    {users.map(u => (
                        <option key={u.userId} value={u.userId}>{u.name}</option>
                    ))}
                </select>
            </div>

            {loading ? (
                <div className="page-loader">Loading tasks...</div>
            ) : filteredTasks.length === 0 ? (
                <div className="empty-state">No tasks found for the current selection.</div>
            ) : (
                <div className="table-responsive">
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Project</th>
                                <th>Assignee</th>
                                <th>Priority</th>
                                <th>Status</th>
                                <th>Due Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredTasks.map((t) => (
                                <tr key={t.taskId}>
                                    <td>
                                        <div className="font-semibold">{t.title}</div>
                                        {t.description && <div className="text-muted">{t.description}</div>}
                                    </td>
                                    <td>{t.projectName || getProjectName(t.projectId)}</td>
                                    <td>{t.assigneeName || getAssigneeName(t.assigneeId)}</td>
                                    <td>
                                        <span className={`priority-pill priority-${(t.priority || "Low").toLowerCase()}`}>
                                            {t.priority}
                                        </span>
                                    </td>
                                    <td>
                                        <StatusDropdown
                                            value={t.status}
                                            onChange={(newVal) => handleQuickStatusChange(t, newVal)}
                                        />
                                    </td>
                                    <td>{t.dueDate ? new Date(t.dueDate).toLocaleDateString() : "N/A"}</td>
                                    <td className="actions-cell">
                                        {canCreateOrEdit ? (
                                            <>
                                                <button className="btn-edit-sm" onClick={() => handleOpenModal(t)}>
                                                    Edit
                                                </button>
                                                <button className="btn-delete-sm" onClick={() => handleDelete(t.taskId)}>
                                                    Delete
                                                </button>
                                            </>
                                        ) : (
                                            <span style={{ color: "#94a3b8", fontSize: "0.85rem" }}>Status only</span>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {isModalOpen && (
                <div className="modal-backdrop">
                    <div className="modal-content modal-lg">
                        <div className="modal-header">
                            <h3>{editingTask ? "Edit Task" : "Create New Task"}</h3>
                            <button className="close-btn" onClick={handleCloseModal}>&times;</button>
                        </div>
                        <form onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label>Task Title</label>
                                <input
                                    type="text"
                                    placeholder="Enter task title"
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>Description</label>
                                <textarea
                                    rows="3"
                                    placeholder="Enter details..."
                                    value={description}
                                    onChange={(e) => setDescription(e.target.value)}
                                />
                            </div>

                            <div className="form-row">
                                <div className="form-group col-6">
                                    <label>Project {isManager && "(Assigned to You)"}</label>
                                    {(() => {
                                        const availableProjects = isManager
                                            ? projects.filter(p => p.ownerId === parseInt(currentUserId))
                                            : projects;

                                        if (availableProjects.length > 0) {
                                            return (
                                                <select
                                                    value={projectId}
                                                    onChange={(e) => setProjectId(e.target.value)}
                                                    required
                                                >
                                                    {availableProjects.map(p => (
                                                        <option key={p.projectId} value={p.projectId}>{p.projectName}</option>
                                                    ))}
                                                </select>
                                            );
                                        } else if (isManager) {
                                            return (
                                                <div style={{ color: "#ef4444", fontSize: "0.85rem", padding: "0.5rem 0" }}>
                                                    ⚠️ No projects assigned to you by Admin.
                                                </div>
                                            );
                                        } else {
                                            return (
                                                <input
                                                    type="number"
                                                    placeholder="Project ID"
                                                    value={projectId}
                                                    onChange={(e) => setProjectId(e.target.value)}
                                                    required
                                                />
                                            );
                                        }
                                    })()}
                                </div>

                                <div className="form-group col-6">
                                    {!editingTask && isManager ? (
                                        <>
                                            <label>Assignee (Employees Only)</label>
                                            {employeeUsers.length > 0 ? (
                                                <select
                                                    value={assigneeId}
                                                    onChange={(e) => setAssigneeId(e.target.value)}
                                                    required
                                                >
                                                    {employeeUsers.map(u => (
                                                        <option key={u.userId} value={u.userId}>{u.name} ({u.email})</option>
                                                    ))}
                                                </select>
                                            ) : (
                                                <div style={{ color: "#ef4444", fontSize: "0.85rem", padding: "0.5rem 0" }}>
                                                    ⚠️ No users with Employee role found. Tasks can only be assigned to Employees.
                                                </div>
                                            )}
                                        </>
                                    ) : !editingTask && isAdmin ? (
                                        <>
                                            <label>Assignee (Managers Only)</label>
                                            {managerUsers.length > 0 ? (
                                                <select
                                                    value={assigneeId}
                                                    onChange={(e) => setAssigneeId(e.target.value)}
                                                    required
                                                >
                                                    {managerUsers.map(u => (
                                                        <option key={u.userId} value={u.userId}>{u.name} ({u.email})</option>
                                                    ))}
                                                </select>
                                            ) : (
                                                <div style={{ color: "#ef4444", fontSize: "0.85rem", padding: "0.5rem 0" }}>
                                                    ⚠️ No users with Manager role found. Tasks can only be assigned to Managers.
                                                </div>
                                            )}
                                        </>
                                    ) : (
                                        <>
                                            <label>Assignee</label>
                                            {users.length > 0 ? (
                                                <select
                                                    value={assigneeId}
                                                    onChange={(e) => setAssigneeId(e.target.value)}
                                                    required
                                                >
                                                    {users.map(u => (
                                                        <option key={u.userId} value={u.userId}>{u.name}</option>
                                                    ))}
                                                </select>
                                            ) : (
                                                <input
                                                    type="number"
                                                    placeholder="Assignee User ID"
                                                    value={assigneeId}
                                                    onChange={(e) => setAssigneeId(e.target.value)}
                                                    required
                                                />
                                            )}
                                        </>
                                    )}
                                </div>
                            </div>

                            <div className="form-row">
                                <div className="form-group col-4">
                                    <label>Status</label>
                                    <StatusDropdown
                                        value={status}
                                        onChange={(newVal) => setStatus(newVal)}
                                        className="modal-status-dropdown-wrapper"
                                    />
                                </div>

                                <div className="form-group col-4">
                                    <label>Priority</label>
                                    <select
                                        value={priority}
                                        onChange={(e) => setPriority(e.target.value)}
                                        required
                                    >
                                        <option value="Low">Low</option>
                                        <option value="Medium">Medium</option>
                                        <option value="High">High</option>
                                        <option value="Urgent">Urgent</option>
                                    </select>
                                </div>

                                <div className="form-group col-4">
                                    <label>Due Date</label>
                                    <input
                                        type="date"
                                        value={dueDate}
                                        onChange={(e) => setDueDate(e.target.value)}
                                        required
                                    />
                                </div>
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={handleCloseModal}>
                                    Cancel
                                </button>
                                <button type="submit" className="btn-primary" disabled={submitting}>
                                    {submitting ? "Saving..." : (editingTask ? "Update" : "Create")}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Tasks;
