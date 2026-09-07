import { useState, useEffect } from "react";
import { getAllProjects, createProject, updateProject, deleteProject } from "../api/projectApi";
import { getAllUsers } from "../api/userApi";
import { useAuth } from "../context/AuthContext";

const Projects = () => {
    const { userId: currentUserId, userRole } = useAuth();
    const isAdmin = userRole === "Admin" || userRole === 3;
    const isManagerOrAdmin = userRole === "Manager" || userRole === "Admin" || userRole === 2 || userRole === 3;
    const [projects, setProjects] = useState([]);
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [searchQuery, setSearchQuery] = useState("");

    // Modal State
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingProject, setEditingProject] = useState(null);
    const [projectName, setProjectName] = useState("");
    const [ownerId, setOwnerId] = useState("");
    const [submitting, setSubmitting] = useState(false);

    // Only managers are available for Admin when creating project
    const managerUsers = users.filter(u => u.role === "Manager" || u.role === 2);
    const availableOwners = isAdmin ? managerUsers : users;

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [projectsData, usersData] = await Promise.all([
                getAllProjects(),
                getAllUsers().catch(() => [])
            ]);
            setProjects(projectsData || []);
            setUsers(usersData || []);
        } catch (err) {
            console.error(err);
            setError("Failed to load projects.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (project = null) => {
        setEditingProject(project);
        if (project) {
            setProjectName(project.projectName || "");
            setOwnerId(project.ownerId || "");
        } else {
            setProjectName("");
            const defaultOwner = isAdmin
                ? (managerUsers.length > 0 ? managerUsers[0].userId : "")
                : (currentUserId || (users.length > 0 ? users[0].userId : ""));
            setOwnerId(defaultOwner);
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingProject(null);
        setProjectName("");
        setOwnerId("");
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSubmitting(true);
        setError("");

        try {
            if (editingProject) {
                await updateProject(editingProject.projectId, { projectName });
            } else {
                if (isAdmin && managerUsers.length === 0) {
                    throw new Error("Cannot create project: No user with Manager role found.");
                }
                await createProject({
                    projectName,
                    ownerId: parseInt(ownerId) || (availableOwners.length > 0 ? availableOwners[0].userId : 1)
                });
            }
            handleCloseModal();
            loadData();
        } catch (err) {
            console.error(err);
            setError(err.response?.data?.message || err.response?.data?.Message || err.message || "Failed to save project.");
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this project?")) return;
        try {
            await deleteProject(id);
            loadData();
        } catch (err) {
            console.error(err);
            setError(err.response?.data?.message || err.response?.data?.Message || "Failed to delete project.");
        }
    };

    const filteredProjects = projects.filter(p =>
        p.projectName?.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const getOwnerName = (id) => {
        const owner = users.find(u => u.userId === id);
        return owner ? owner.name : `User #${id}`;
    };

    return (
        <div className="page-container">
            <div className="page-header-actions">
                <div>
                    <h2>Projects</h2>
                    <p>Manage and track all ongoing organization projects.</p>
                </div>
                {isAdmin && (
                    <button className="btn-primary" onClick={() => handleOpenModal()}>
                        + New Project
                    </button>
                )}
            </div>

            {error && <div className="error-alert">{error}</div>}

            <div className="toolbar">
                <input
                    type="text"
                    className="search-input"
                    placeholder="Search projects..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                />
            </div>

            {loading ? (
                <div className="page-loader">Loading projects...</div>
            ) : filteredProjects.length === 0 ? (
                <div className="empty-state">No projects found.</div>
            ) : (
                <div className="grid-cards">
                    {filteredProjects.map((p) => (
                        <div key={p.projectId} className="project-card">
                            <div className="project-card-header">
                                <span className="project-icon">📁</span>
                                <h3>{p.projectName}</h3>
                            </div>
                            <div className="project-card-body">
                                <p className="project-owner">
                                    <strong>Owner:</strong> {p.ownerName || getOwnerName(p.ownerId)}
                                </p>
                                <span className="project-id-tag">ID #{p.projectId}</span>
                            </div>
                            {isAdmin && (
                                <div className="project-card-actions">
                                    <button className="btn-edit" onClick={() => handleOpenModal(p)}>
                                        Edit
                                    </button>
                                    <button className="btn-delete" onClick={() => handleDelete(p.projectId)}>
                                        Delete
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}

            {isModalOpen && (
                <div className="modal-backdrop">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>{editingProject ? "Edit Project" : "Create New Project"}</h3>
                            <button className="close-btn" onClick={handleCloseModal}>&times;</button>
                        </div>
                        <form onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label>Project Name</label>
                                <input
                                    type="text"
                                    placeholder="Enter project title"
                                    value={projectName}
                                    onChange={(e) => setProjectName(e.target.value)}
                                    required
                                />
                            </div>

                            {!editingProject && (
                                <div className="form-group">
                                    <label>Owner {isAdmin && "(Managers Only)"}</label>
                                    {availableOwners.length > 0 ? (
                                        <select
                                            value={ownerId}
                                            onChange={(e) => setOwnerId(e.target.value)}
                                            required
                                        >
                                            {availableOwners.map(u => (
                                                <option key={u.userId} value={u.userId}>
                                                    {u.name} ({u.email})
                                                </option>
                                            ))}
                                        </select>
                                    ) : (
                                        <div style={{ color: "#ef4444", fontSize: "0.85rem", padding: "0.5rem 0" }}>
                                            {isAdmin
                                                ? "⚠️ No users with Manager role found. Only managers can be assigned as project owners."
                                                : <input
                                                    type="number"
                                                    placeholder="Enter Owner User ID"
                                                    value={ownerId}
                                                    onChange={(e) => setOwnerId(e.target.value)}
                                                    required
                                                />
                                            }
                                        </div>
                                    )}
                                </div>
                            )}

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={handleCloseModal}>
                                    Cancel
                                </button>
                                <button type="submit" className="btn-primary" disabled={submitting}>
                                    {submitting ? "Saving..." : (editingProject ? "Update" : "Create")}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Projects;
