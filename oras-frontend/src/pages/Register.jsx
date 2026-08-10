
import { useState } from "react";
import { registerUser } from "../api/authApi";

import { Link } from "react-router-dom";


const Register = () => {
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e) => {
    e.preventDefault();

    try {
        const response = await registerUser({
            name,
            email,
            password
        });

        console.log("Registration successful:", response);

    } catch (error) {
        console.error("Registration failed:", error);
    }
};

    return (
        <div className="auth-container">
            <div className="auth-card">

                <h1>Create Account</h1>
                <p>Create your ORAS account</p>

                <form onSubmit={handleSubmit}>

                    <div className="form-group">
                        <label>Name</label>

                        <input
                            type="text"
                            placeholder="Enter your name"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            required
                        />
                    </div>

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
                        Register
                    </button>

                </form>


                <p className="auth-link">
                    Already have an account?{" "}
                    <Link to="/login">Login</Link>
                </p>

            </div>
        </div>
    );
};

export default Register;