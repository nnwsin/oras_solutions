import { createContext, useContext, useState, useEffect } from "react";

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [token, setToken] = useState(() => localStorage.getItem("token"));
    const [user, setUser] = useState(() => {
        const storedUser = localStorage.getItem("user");
        if (storedUser) {
            try { return JSON.parse(storedUser); } catch { return null; }
        }
        const storedEmail = localStorage.getItem("userEmail");
        return storedEmail ? { email: storedEmail } : null;
    });

    const login = (authResponse, emailFallback) => {
        const tokenStr = typeof authResponse === "string" ? authResponse : authResponse?.token;
        setToken(tokenStr);
        localStorage.setItem("token", tokenStr);

        let userObj = null;
        if (typeof authResponse === "object" && authResponse.userId) {
            userObj = {
                userId: authResponse.userId,
                name: authResponse.name,
                email: authResponse.email,
                role: authResponse.role
            };
        } else {
            userObj = { email: emailFallback || "" };
        }

        setUser(userObj);
        localStorage.setItem("user", JSON.stringify(userObj));
        if (userObj.email) localStorage.setItem("userEmail", userObj.email);
    };

    const logout = () => {
        setToken(null);
        setUser(null);
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        localStorage.removeItem("userEmail");
    };

    return (
        <AuthContext.Provider value={{
            token,
            user,
            userId: user?.userId || null,
            userEmail: user?.email || "",
            userRole: user?.role || null,
            login,
            logout,
            isAuthenticated: !!token
        }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
};
