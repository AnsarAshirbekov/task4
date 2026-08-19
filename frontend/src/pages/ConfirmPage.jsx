import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { confirmEmail } from "../api/api";

function ConfirmPage() {
    const [searchParams] = useSearchParams();

    const started = useRef(false);

    const [status, setStatus] = useState("loading");
    const [message, setMessage] = useState("");

    useEffect(() => {
        if (started.current) return;
        started.current = true;
        const token = searchParams.get("token");

        if (!token) {
            setStatus("error");
            setMessage("Confirmation token is missing");
            return;
        }

        async function confirm() {
            try {
                await confirmEmail(token);

                setStatus("success");
                setMessage("Email confirmed successfully");
            } catch (error) {
                setStatus("error");
                setMessage(error.message);
            }
        }

        confirm();
    }, [searchParams]);

    return (
        <div className="auth-page">
            <div className="auth-card text-center">

                {status === "loading" && (
                    <>
                        <h1 className="h3">
                            Confirming email
                        </h1>

                        <p className="text-secondary mb-0">
                            Please wait...
                        </p>
                    </>
                )}

                {status === "success" && (
                    <>
                        <h1 className="h3 text-success">
                            Email confirmed
                        </h1>

                        <p>
                            Your email has been confirmed successfully.
                        </p>

                        <Link
                            to="/login"
                            className="btn btn-primary"
                        >
                            Go to login
                        </Link>
                    </>
                )}

                {status === "error" && (
                    <>
                        <h1 className="h3 text-danger">
                            Confirmation failed
                        </h1>

                        <div className="alert alert-danger">
                            {message}
                        </div>

                        <Link
                            to="/login"
                            className="btn btn-outline-primary"
                        >
                            Go to login
                        </Link>
                    </>
                )}

            </div>
        </div>
    );
}

export default ConfirmPage;