import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Navbar from "../components/Navbar";
import UserToolbar from "../components/UserToolbar";
import UserTable from "../components/UserTable";

import {
    getUsers,
    blockUsers,
    unblockUsers,
    deleteUsers
} from "../api/api";

function UsersPage() {
    const navigate = useNavigate();

    const [users, setUsers] = useState([]);
    const [selectedIds, setSelectedIds] = useState([]);

    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState(false);

    const [message, setMessage] = useState(null);

    function showMessage(type, text) {
        setMessage({ type, text });
    }

    async function loadUsers() {
        try {
            setLoading(true);

            const data = await getUsers();

            setUsers(data);
            setSelectedIds([]);
        } catch (error) {
            if (error.status === 401) {
                navigate("/login");
                return;
            }

            showMessage("danger", error.message);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        loadUsers();
    }, []);

    async function handleBlock() {
        await performAction(
            () => blockUsers(selectedIds),
            "Users blocked"
        );
    }

    async function handleUnblock() {
        await performAction(
            () => unblockUsers(selectedIds),
            "Users unblocked"
        );
    }

    async function handleDelete() {
        await performAction(
            () => deleteUsers(selectedIds),
            "Users deleted"
        );
    }

    async function handleDeleteUnverified() {
        const ids = users
            .filter(user => user.status === "unverified")
            .map(user => user.id);

        if (ids.length === 0) {
            showMessage(
                "info",
                "There are no unverified users"
            );
            return;
        }

        await performAction(
            () => deleteUsers(ids),
            "Unverified users deleted"
        );
    }

    async function performAction(action, successMessage) {
        try {
            setActionLoading(true);

            const result = await action();

            if (result?.redirectToLogin) {
                navigate("/login");
                return;
            }

            showMessage("success", successMessage);

            await loadUsers();
        } catch (error) {
            if (error.status === 401) {
                navigate("/login");
                return;
            }

            showMessage("danger", error.message);
        } finally {
            setActionLoading(false);
        }
    }

    return (
        <>
            <Navbar onMessage={showMessage} />

            <main className="container py-4">
                <div className="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h1 className="h3 mb-1">
                            User management
                        </h1>

                        <p className="text-secondary mb-0">
                            Manage registered users
                        </p>
                    </div>
                </div>
                
                <UserToolbar
                    selectedCount={selectedIds.length}
                    onBlock={handleBlock}
                    onUnblock={handleUnblock}
                    onDelete={handleDelete}
                    onDeleteUnverified={handleDeleteUnverified}
                    disabled={loading || actionLoading}
                />

                {loading ? (
                    <div className="text-center py-5">
                        <div
                            className="spinner-border"
                            role="status"
                        >
                            <span className="visually-hidden">
                                Loading...
                            </span>
                        </div>
                    </div>
                ) : (
                    <UserTable
                        users={users}
                        selectedIds={selectedIds}
                        onSelectionChange={setSelectedIds}
                    />
                )}
                <div className="status-message">
                    {message && (
                        <div className={`alert alert-${message.type} mb-0`}>
                            <div className="d-flex justify-content-between align-items-center">
                                <span>{message.text}</span>

                                <button
                                    type="button"
                                    className="btn-close"
                                    aria-label="Close"
                                    onClick={() => setMessage(null)}
                                />
                            </div>
                        </div>
                    )}
                </div>
            </main>
        </>
    );
}
export default UsersPage;