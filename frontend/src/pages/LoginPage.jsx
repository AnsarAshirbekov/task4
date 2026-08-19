import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/api";

function LoginPage() {
    const navigate = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(event) {
        event.preventDefault();

        setError("");
        setLoading(true);

        try {
            await login({
                email,
                password
            });

            navigate("/users");
        } catch (error) {
            setError(error.message);
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="auth-page">
            <div className="auth-page">
                <form className="auth-card" onSubmit={handleSubmit}>
                    <h1 className="h3 text-center">Sign in</h1>

                    <div className="mb-3">
                        <label className="form-label">
                            Email
                        </label>

                        <input
                            type="email"
                            className="form-control"
                            value={email}
                            onChange={event => setEmail(event.target.value)}
                            required
                        />
                    </div>

                    <div className="mb-3">
                        <label className="form-label">
                            Password
                        </label>

                        <input
                            type="password"
                            className="form-control"
                            value={password}
                            onChange={event => setPassword(event.target.value)}
                            required
                        />
                    </div>

                    <div className="status-message">
                        {error && (
                            <div className="alert alert-danger mb-0">
                                {error}
                            </div>
                        )}
                    </div>

                    <button
                        type="submit"
                        className="btn btn-primary w-100"
                        disabled={loading}
                    >
                        {loading ? "Signing in..." : "Sign in"}
                    </button>

                    <div className="auth-links">
                        <span className="text-secondary">
                            Don't have an account?{" "}
                        </span>

                        <Link to="/register">
                            Register
                        </Link>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default LoginPage;