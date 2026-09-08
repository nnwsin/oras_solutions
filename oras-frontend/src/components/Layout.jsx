import { useState, useRef, useEffect } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";

const Layout = () => {
    const { user, userEmail, userRole, logout } = useAuth();
    const { theme, toggleTheme, isDark } = useTheme();
    const navigate = useNavigate();
    const [isProfileOpen, setIsProfileOpen] = useState(false);
    const profileRef = useRef(null);

    const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(() => {
        return localStorage.getItem("oras_sidebar_collapsed") === "true";
    });

    const toggleSidebar = () => {
        setIsSidebarCollapsed((prev) => {
            const next = !prev;
            localStorage.setItem("oras_sidebar_collapsed", next ? "true" : "false");
            return next;
        });
    };

    const handleLogout = () => {
        setIsProfileOpen(false);
        logout();
        navigate("/login");
    };

    // Close dropdown on outside click or Escape key
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (profileRef.current && !profileRef.current.contains(e.target)) {
                setIsProfileOpen(false);
            }
        };
        const handleKeyDown = (e) => {
            if (e.key === "Escape") {
                setIsProfileOpen(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        document.addEventListener("keydown", handleKeyDown);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
            document.removeEventListener("keydown", handleKeyDown);
        };
    }, []);

    const isAdmin = userRole === "Admin" || userRole === 3;
    const isManager = userRole === "Manager" || userRole === 2;
    const roleName = isAdmin ? "Admin" : (isManager ? "Manager" : "Employee");

    const displayName = user?.name || (userEmail ? userEmail.split("@")[0] : "Admin");
    const firstLetter = (displayName ? displayName.trim()[0] : "A").toUpperCase();

    return (
        <div className={`dashboard ${isSidebarCollapsed ? "sidebar-collapsed" : ""}`}>
            <aside className={`sidebar ${isSidebarCollapsed ? "collapsed" : ""}`}>
                <div className="sidebar-brand">
                    <div className="sidebar-brand-content">
                        <div className="sidebar-brand-text">
                            <h2>ORAS</h2>
                            <span className="brand-subtitle">Task & Project Suite</span>
                        </div>
                        <button
                            className="sidebar-collapse-btn"
                            onClick={toggleSidebar}
                            title="Collapse sidebar"
                            id="collapse-sidebar-btn"
                        >
                            ◀
                        </button>
                    </div>
                </div>

                <nav className="sidebar-nav">
                    <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">📊</span>
                        <span className="nav-text">Dashboard</span>
                    </NavLink>
                    <NavLink to="/projects" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">📁</span>
                        <span className="nav-text">Projects</span>
                    </NavLink>
                    <NavLink to="/tasks" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">✅</span>
                        <span className="nav-text">Tasks</span>
                    </NavLink>
                    <NavLink to="/comments" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">💬</span>
                        <span className="nav-text">Comments</span>
                    </NavLink>
                    <NavLink to="/users" className={({ isActive }) => (isActive ? "active" : "")}>
                        <span className="nav-icon">👥</span>
                        <span className="nav-text">Users</span>
                    </NavLink>
                </nav>
            </aside>

            <main className="dashboard-main">
                <header className="dashboard-header">
                    <div className="header-title">
                        <button
                            className="header-menu-btn"
                            onClick={toggleSidebar}
                            title={isSidebarCollapsed ? "Open sidebar" : "Collapse sidebar"}
                            id="header-sidebar-toggle"
                        >
                            ☰
                        </button>
                        <h1>ORAS Solutions</h1>
                    </div>

                    <div className="header-actions">
                        {/* Top Right Profile Section - 1st character of profile name */}
                        <div className="profile-section" ref={profileRef}>
                            <button
                                className={`profile-trigger ${isProfileOpen ? "active" : ""}`}
                                onClick={() => setIsProfileOpen(!isProfileOpen)}
                                id="profile-menu-btn"
                                aria-expanded={isProfileOpen}
                                aria-label="User Profile Menu"
                                title={displayName}
                            >
                                <div className="profile-avatar">{firstLetter}</div>
                            </button>

                            {/* Simple Profile Dropdown Menu */}
                            {isProfileOpen && (
                                <div className="profile-dropdown animate-fade-in" id="profile-dropdown">
                                    <div className="dropdown-user-header">
                                        <div className="dropdown-avatar">{firstLetter}</div>
                                        <div className="dropdown-user-details">
                                            <p className="dropdown-name">{displayName}</p>
                                            <p className="dropdown-email">{userEmail || ""}</p>
                                        </div>
                                    </div>

                                    <div className="dropdown-divider" />

                                    <div className="dropdown-menu-list">
                                        {/* Simple Change Theme Button */}
                                        <button
                                            className="dropdown-item"
                                            onClick={toggleTheme}
                                            id="dropdown-change-theme-btn"
                                        >
                                            <span className="dropdown-item-icon">{isDark ? "☀️" : "🌙"}</span>
                                            <span>Change Theme</span>
                                            <span className="theme-indicator-pill" style={{ marginLeft: "auto" }}>
                                                {isDark ? "Dark" : "Light"}
                                            </span>
                                        </button>

                                        {/* Simple Logout Button */}
                                        <button
                                            className="dropdown-item logout-item"
                                            onClick={handleLogout}
                                            id="profile-logout-btn"
                                        >
                                            <span className="dropdown-item-icon">🚪</span>
                                            <span>Logout</span>
                                        </button>
                                    </div>
                                </div>
                            )}
                        </div>
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
