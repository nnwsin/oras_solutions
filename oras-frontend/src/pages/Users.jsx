import { useState, useEffect } from "react";
import { getAllUsers, createUser, updateUser, deleteUser } from "../api/userApi";
import { useAuth } from "../context/AuthContext";

const Users = () => {
    const { userRole } = useAuth();
    const isAdmin = userRole === "Admin" || userRole === 3;
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [searchQuery, setSearchQuery] = useState("");

    // Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingUser, setEditingUser] = useState(null);
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [role, setRole] = useState(1);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        loadUsers();
    }, []);

    const loadUsers = async () => {
        try {
            setLoading(true);
            const data = await getAllUsers();
            setUsers(data || []);
        } catch (err) {
            console.error(err);
            setError("Failed to load user directory.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (usr = null) => {
        setEditingUser(usr);
        if (usr) {
            setName(usr.name || "");
            setEmail(usr.email || "");
            setPassword("");
            setRole(usr.role || 1);
        } else {
            setName("");
            setEmail("");
            setPassword("");
            setRole(1);
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingUser(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setSubmitting(true);
        setError("");

        try {
            if (editingUser) {
                await updateUser(editingUser.userId, {
                    name,
                    email,
                    role: parseInt(role)
                });
            } else {
                await createUser({
                    name,
                    email,
                    password,
                    role: parseInt(role)
                });
            }
            handleCloseModal();
            loadUsers();
        } catch (err) {
            console.error(err);
            setError(err.response?.data?.message || err.response?.data?.Message || "Failed to save user details.");
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Delete this user?")) return;
        try {
            await deleteUser(id);
            loadUsers();
        } catch (err) {
            console.error(err);
            setError(err.response?.data?.message || err.response?.data?.Message || "Failed to delete user.");
        }
    };

    const getRoleName = (roleVal) => {
        switch (roleVal) {
            case 1:
            case "Employee":
                return "Employee";
            case 2:
            case "Manager":
                return "Manager";
            case 3:
            case "Admin":
                return "Admin";
            default:
                return roleVal;
        }
    };

    const getRoleBadgeClass = (roleVal) => {
        switch (roleVal) {
            case 3:
            case "Admin":
                return "badge-admin";
            case 2:
            case "Manager":
                return "badge-manager";
            default:
                return "badge-employee";
        }
    };

    const filteredUsers = users.filter(u =>
        u.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        u.email?.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
        <div className="page-container">
            <div className="page-header-actions">
                <div>
                    <h2>Team Members</h2>
                    <p>Manage member roles and permissions within ORAS.</p>
                </div>
                {isAdmin && (
                    <button className="btn-primary" onClick={() => handleOpenModal()}>
                        + Add Member
                    </button>
                )}
            </div>

            {error && <div className="error-alert">{error}</div>}

            <div className="toolbar">
                <input
                    type="text"
                    className="search-input"
                    placeholder="Search by name or email..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                />
            </div>

            {loading ? (
                <div className="page-loader">Loading team directory...</div>
            ) : filteredUsers.length === 0 ? (
                <div className="empty-state">No team members found.</div>
            ) : (
                <div className="table-responsive">
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>User ID</th>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                {isAdmin && <th>Actions</th>}
                            </tr>
                        </thead>
                        <tbody>
                            {filteredUsers.map((u) => (
                                <tr key={u.userId}>
                                    <td className="font-mono">#{u.userId}</td>
                                    <td className="font-semibold">{u.name}</td>
                                    <td>{u.email}</td>
                                    <td>
                                        <span className={`role-badge ${getRoleBadgeClass(u.role)}`}>
                                            {getRoleName(u.role)}
                                        </span>
                                    </td>
                                    {isAdmin && (
                                        <td className="actions-cell">
                                            <button className="btn-edit-sm" onClick={() => handleOpenModal(u)}>
                                                Edit
                                            </button>
                                            <button className="btn-delete-sm" onClick={() => handleDelete(u.userId)}>
                                                Delete
                                            </button>
                                        </td>
                                    )}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {isModalOpen && (
                <div className="modal-backdrop">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>{editingUser ? "Edit User" : "Create New User"}</h3>
                            <button className="close-btn" onClick={handleCloseModal}>&times;</button>
                        </div>
                        <form onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label>Full Name</label>
                                <input
                                    type="text"
                                    placeholder="Enter full name"
                                    value={name}
                                    onChange={(e) => setName(e.target.value)}
                                    required
                                />
                            </div>

                            <div className="form-group">
                                <label>Email Address</label>
                                <input
                                    type="email"
                                    placeholder="name@example.com"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                />
                            </div>

                            {!editingUser && (
                                <div className="form-group">
                                    <label>Password</label>
                                    <input
                                        type="password"
                                        placeholder="Min 6 characters"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        minLength={6}
                                        required
                                    />
                                </div>
                            )}

                            <div className="form-group">
                                <label>Role</label>
                                <select
                                    value={role}
                                    onChange={(e) => setRole(e.target.value)}
                                    required
                                >
                                    <option value={1}>Employee</option>
                                    <option value={2}>Manager</option>
                                    <option value={3}>Admin</option>
                                </select>
                            </div>

                            <div className="modal-actions">
                                <button type="button" className="btn-secondary" onClick={handleCloseModal}>
                                    Cancel
                                </button>
                                <button type="submit" className="btn-primary" disabled={submitting}>
                                    {submitting ? "Saving..." : (editingUser ? "Update" : "Create Member")}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Users;
