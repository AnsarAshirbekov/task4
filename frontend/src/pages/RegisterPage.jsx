import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../api/api";

function RegisterPage() {
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const [message, setMessage] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(event) {
        event.preventDefault();

        setError("");
        setMessage("");
        setLoading(true);

        try {
            const result = await register({
                name,
                email,
                password
            });

            setMessage(result.message);
        } catch (error) {
            setError(error.message);
        } finally {
            setLoading(false);
        }
    }

    return (
        <div className="auth-page">
            <form className="auth-card" onSubmit={handleSubmit}>
                <h1 className="h3 text-center">
                    Create account
                </h1>

                <div className="mb-3">
                    <label className="form-label">
                        Name
                    </label>

                    <input
                        type="text"
                        className="form-control"
                        value={name}
                        onChange={event => setName(event.target.value)}
                        required
                    />
                </div>

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

                    {!error && message && (
                        <div className="alert alert-success mb-0">
                            {message}
                        </div>
                    )}
                </div>

                <button
                    type="submit"
                    className="btn btn-primary w-100"
                    disabled={loading}
                >
                    {loading ? "Creating account..." : "Register"}
                </button>

                <div className="auth-links">
                    <span className="text-secondary">
                        Already have an account?{" "}
                    </span>

                    <Link to="/login">
                        Sign in
                    </Link>
                </div>
            </form>
        </div>
    );
}

export default RegisterPage;