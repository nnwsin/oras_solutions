import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { registerUser } from "../api/authApi";
import { useTheme } from "../context/ThemeContext";

const Register = () => {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [loading, setLoading] = useState(false);

    const { isDark, toggleTheme } = useTheme();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");
        setSuccess("");
        setLoading(true);

        try {
            const response = await registerUser({ name, email, password });
            setSuccess(response.message || "Registration successful! Redirecting to login...");
            setTimeout(() => {
                navigate("/login");
            }, 1500);
        } catch (err) {
            console.error("Registration failed:", err);
            const msg = err.response?.data?.message || err.response?.data?.Message || "Registration failed. Please try again.";
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
                    <h1>Create Account</h1>
                    <p>Get started with ORAS project management</p>
                </div>

                {error && <div className="error-alert">{error}</div>}
                {success && <div className="success-alert">{success}</div>}

                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Full Name</label>
                        <input
                            type="text"
                            placeholder="John Doe"
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

                    <div className="form-group">
                        <label>Password</label>
                        <input
                            type="password"
                            placeholder="At least 6 characters"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            minLength={6}
                            required
                        />
                    </div>

                    <button type="submit" className="btn-primary" disabled={loading}>
                        {loading ? "Registering..." : "Create Account"}
                    </button>
                </form>

                <p className="auth-link">
                    Already have an account? <Link to="/login">Login here</Link>
                </p>
            </div>
        </div>
    );
};

export default Register;