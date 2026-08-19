function UserToolbar({
    selectedCount,
    onBlock,
    onUnblock,
    onDelete,
    onDeleteUnverified,
    disabled
}) {
    return (
        <div className="d-flex flex-wrap gap-2 align-items-center mb-3">
            <button
                className="btn btn-primary"
                disabled={disabled || selectedCount === 0}
                onClick={onBlock}
            >
                Block
            </button>

            <button
                className="btn btn-outline-secondary"
                disabled={disabled || selectedCount === 0}
                onClick={onUnblock}
                title="Unblock selected users"
            >
                <i className="bi bi-unlock"></i>
            </button>

            <button
                className="btn btn-outline-danger"
                disabled={disabled || selectedCount === 0}
                onClick={onDelete}
                title="Delete selected users"
            >
                <i className="bi bi-trash"></i>
            </button>

            <button
                className="btn btn-outline-danger"
                disabled={disabled}
                onClick={onDeleteUnverified}
                title="Delete all unverified users"
            >
                <i className="bi bi-person-x"></i>
            </button>

            <span className="text-secondary ms-2">
                Selected: {selectedCount}
            </span>
        </div>
    );
}

export default UserToolbar;