import { useState } from "react";
import { loginUser } from "../api/authApi";

import { Link } from "react-router-dom";



const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await loginUser({
                email,
                password
            });

            console.log("Login successful:", response);

        } catch (error) {
            console.error("Login failed:", error);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-card">

                <h1>Welcome Back</h1>

                <p>Login to your ORAS account</p>

                <form onSubmit={handleSubmit}>

                    <div className="form-group">
                        <label>Email</label>

                        <input
                            type="email"
                            placeholder="Enter your email"
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

                    <button type="submit">
                        Login
                    </button>

                </form>

                <p className="auth-link">
                    Don't have an account?{" "}
                    <Link to="/register">Register</Link>
                </p>

            </div>
        </div>
    );
};

export default Login;