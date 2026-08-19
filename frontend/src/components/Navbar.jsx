import { useNavigate } from "react-router-dom";
import { logout } from "../api/api";

function Navbar({ onMessage }) {
    const navigate = useNavigate();

    async function handleLogout() {
        try {
            await logout();
            navigate("/login");
        } catch (error) {
            onMessage({
                type: "danger",
                text: error.message
            });
        }
    }

    return (
        <nav className="navbar navbar-light bg-white border-bottom px-4">
            <div className="container-fluid">
                <span className="navbar-brand fw-semibold">
                    Task 4
                </span>

                <button
                    className="btn btn-outline-secondary btn-sm"
                    onClick={handleLogout}
                >
                    Logout
                </button>
            </div>
        </nav>
    );
}

export default Navbar;