function getUniqIdValue(user) {
    return user.id;
}

function UserTable({
    users,
    selectedIds,
    onSelectionChange
}) {
    const allSelected =
        users.length > 0 &&
        users.every(user =>
            selectedIds.includes(getUniqIdValue(user))
        );

    function toggleAll() {
        if (allSelected) {
            onSelectionChange([]);
            return;
        }

        onSelectionChange(
            users.map(getUniqIdValue)
        );
    }

    function toggleUser(id) {
        if (selectedIds.includes(id)) {
            onSelectionChange(
                selectedIds.filter(selectedId => selectedId !== id)
            );
        } else {
            onSelectionChange([
                ...selectedIds,
                id
            ]);
        }
    }

    return (
        <div className="table-responsive user-table-container">
            <table className="table table-hover align-middle mb-0">
                <thead className="table-light">
                    <tr>
                        <th style={{ width: "48px" }}>
                            <input
                                className="form-check-input"
                                type="checkbox"
                                checked={allSelected}
                                onChange={toggleAll}
                                aria-label="Select all users"
                            />
                        </th>

                        <th>Name</th>
                        <th>Email</th>
                        <th>Last login</th>
                        <th>Status</th>
                    </tr>
                </thead>

                <tbody>
                    {users.map(user => {
                        const id = getUniqIdValue(user);

                        return (
                            <tr key={id}>
                                <td>
                                    <input
                                        className="form-check-input"
                                        type="checkbox"
                                        checked={selectedIds.includes(id)}
                                        onChange={() => toggleUser(id)}
                                        aria-label={`Select ${user.name}`}
                                    />
                                </td>

                                <td>{user.name}</td>

                                <td>{user.email}</td>

                                <td>
                                    {user.lastLoginAt
                                        ? new Date(
                                            user.lastLoginAt
                                        ).toLocaleString()
                                        : "Never"}
                                </td>

                                <td>
                                    <span
                                        className={
                                            user.status === "active"
                                                ? "badge text-bg-success"
                                                : user.status === "blocked"
                                                    ? "badge text-bg-danger"
                                                    : "badge text-bg-secondary"
                                        }
                                    >
                                        {user.status}
                                    </span>
                                </td>
                            </tr>
                        );
                    })}
                </tbody>
            </table>
        </div>
    );
}

export default UserTable;