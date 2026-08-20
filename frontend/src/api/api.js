const API = import.meta.env.VITE_API_URL;

async function request(url, options = {}) {
    const response = await fetch(`${API}${url}`, {
        credentials: "include",
        ...options
    });

    const text = await response.text();

    let data = null;

    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }
    }

    if (!response.ok) {
        const error = new Error(
            typeof data === "string"
                ? data
                : data?.message || "Request failed"
        );

        error.status = response.status;

        throw error;
    }

    return data;
}

export function register(data) {
    return request("/api/auth/register", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });
}

export function login(data) {
    return request("/api/auth/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });
}

export function logout() {
    return request("/api/auth/logout", {
        method: "POST"
    });
}

export function getUsers() {
    return request("/api/users");
}

export function blockUsers(userIds) {
    return request("/api/users/block", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ userIds })
    });
}

export function unblockUsers(userIds) {
    return request("/api/users/unblock", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ userIds })
    });
}

export function deleteUsers(userIds) {
    return request("/api/users", {
        method: "DELETE",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ userIds })
    });
}

export function confirmEmail(token) {
    return request(
        `/api/auth/confirm?token=${encodeURIComponent(token)}`,
        {
            credentials: "omit"
        }
    );
}