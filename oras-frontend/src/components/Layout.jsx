import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const Layout = () => {
    const { userEmail, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <div className="dashboard">
            <aside className="sidebar">
                <div className="sidebar-brand">
                    <h2>ORAS</h2>
                    <span className="brand-subtitle">Task & Project Suite</span>
                </div>

                <nav className="sidebar-nav">
                    <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">📊</span> Dashboard
                    </NavLink>
                    <NavLink to="/projects" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">📁</span> Projects
                    </NavLink>
                    <NavLink to="/tasks" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">✅</span> Tasks
                    </NavLink>
                    <NavLink to="/comments" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">💬</span> Comments
                    </NavLink>
                    <NavLink to="/users" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">👥</span> Users
                    </NavLink>
                </nav>

                <div className="sidebar-footer">
                    <button className="logout-btn" onClick={handleLogout}>
                        🚪 Logout
                    </button>
                </div>
            </aside>

            <main className="dashboard-main">
                <header className="dashboard-header">
                    <div className="header-title">
                        <h1>ORAS Management System</h1>
                    </div>

                    <div className="user-info">
                        <span className="user-avatar">👤</span>
                        <span className="user-email">{userEmail || "User"}</span>
                    </div>
                </header>

                <section className="dashboard-content">
                    <Outlet />
                </section>
            </main>
        </div>
    );
};

export default Layout;
