import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { loginUser } from "../api/authApi";
import { useAuth } from "../context/AuthContext";
import { useTheme } from "../context/ThemeContext";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const { login } = useAuth();
    const { isDark, toggleTheme } = useTheme();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");
        setLoading(true);

        try {
            const response = await loginUser({ email, password });
            login(response, email);
            navigate("/dashboard");
        } catch (err) {
            console.error("Login failed:", err);
            const msg = err.response?.data?.message || err.response?.data?.Message || "Login failed. Please check your credentials.";
            setError(msg);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-theme-toggle-wrapper">
                <button
                    type="button"
                    className="auth-theme-toggle-btn"
                    onClick={toggleTheme}
                    title={isDark ? "Switch to Light Theme" : "Switch to Dark Theme"}
                    id="auth-theme-toggle-btn"
                >
                    <span className="theme-toggle-icon">{isDark ? "☀️" : "🌙"}</span>
                    <span className="theme-toggle-label">{isDark ? "Light Mode" : "Dark Mode"}</span>
                </button>
            </div>

            <div className="auth-card">
                <div className="auth-header">
                    <h1>Welcome Back</h1>
                    <p>Login to your ORAS account</p>
                </div>

                {error && <div className="error-alert">{error}</div>}

                <form onSubmit={handleSubmit}>
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

                    <div className="form-group">
                        <label>Password</label>
                        <input
                            type="password"
                            placeholder="Enter your password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                    </div>

                    <button type="submit" className="btn-primary" disabled={loading}>
                        {loading ? "Logging in..." : "Login"}
                    </button>
                </form>

                <p className="auth-link">
                    Don't have an account? <Link to="/register">Register here</Link>
                </p>
            </div>
        </div>
    );
};

export default Login;