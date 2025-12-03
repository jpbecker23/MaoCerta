(function (window, document) {
    const TOKEN_KEY = "maocerta.jwt";
    const USER_KEY = "maocerta.user";
    const CLIENT_KEY = "maocerta.client";
    const PROFESSIONAL_KEY = "maocerta.professional";

    class ApiClientError extends Error {
        constructor(message, status, payload) {
            super(message);
            this.name = "ApiClientError";
            this.status = status;
            this.payload = payload;
        }
    }

    function getBaseUrl() {
        const meta = document.querySelector('meta[name="api-base-url"]');
        const url = meta?.getAttribute('content') ?? '';
        return url.replace(/\/$/, "");
    }

    const baseUrl = getBaseUrl();

    function buildUrl(path) {
        if (!path) {
            return baseUrl;
        }
        if (/^https?:/i.test(path)) {
            return path;
        }
        const sanitized = path.startsWith('/') ? path : `/${path}`;
        return `${baseUrl}${sanitized}`;
    }

    function prepareBody(body, headers) {
        if (!body) {
            return undefined;
        }
        if (body instanceof FormData || body instanceof Blob) {
            return body;
        }
        if (typeof body === "string") {
            headers.set("Content-Type", "application/json");
            return body;
        }
        headers.set("Content-Type", "application/json");
        return JSON.stringify(body);
    }

    async function request(path, options = {}) {
        const { method = "GET", headers = {}, body, skipAuth = false, suppressUnauthorized = false } = options;
        const url = buildUrl(path);
        const mergedHeaders = new Headers(headers);
        mergedHeaders.set("Accept", "application/json");
        const token = localStorage.getItem(TOKEN_KEY);
        if (!skipAuth && token) {
            mergedHeaders.set("Authorization", `Bearer ${token}`);
        }
        const payload = prepareBody(body, mergedHeaders);
        console.debug(`[ApiClient] ${method} ${url}`, { headers: Object.fromEntries(mergedHeaders.entries()), body: payload });
        let response;
        try {
            response = await fetch(url, {
                method,
                body: payload,
                headers: mergedHeaders
            });
        } catch (networkError) {
            console.error(`[ApiClient] Network error calling ${url}`, networkError);
            throw new ApiClientError("Não foi possível conectar ao servidor", 0);
        }

        if (response.status === 401) {
            if (!suppressUnauthorized) {
                clearAuth();
                window.dispatchEvent(new CustomEvent("maocerta:unauthorized"));
            }
            throw new ApiClientError("Não autorizado", response.status);
        }

        if (response.status === 204) {
            return null;
        }

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
            const message = typeof data === "string" ? data : data?.message || "Erro ao comunicar com a API";
            throw new ApiClientError(message, response.status, data);
        }

        return data;
    }

    function get(path, options) {
        return request(path, { ...(options ?? {}), method: "GET" });
    }

    function post(path, body, options) {
        return request(path, { ...(options ?? {}), method: "POST", body });
    }

    function put(path, body, options) {
        return request(path, { ...(options ?? {}), method: "PUT", body });
    }

    function del(path, options) {
        return request(path, { ...(options ?? {}), method: "DELETE" });
    }

    function buildQuery(params = {}) {
        const entries = Object.entries(params)
            .filter(([, value]) => value !== undefined && value !== null && value !== "");
        if (!entries.length) {
            return "";
        }
        return `?${entries.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`).join("&")}`;
    }

    function showNotification(message, type = "info") {
        const bar = document.getElementById("notification-bar");
        if (!bar) {
            return;
        }
        bar.textContent = message;
        bar.dataset.type = type;
        bar.hidden = false;
        window.clearTimeout(bar._timeoutId);
        bar._timeoutId = window.setTimeout(() => {
            bar.hidden = true;
        }, 5000);
    }

    function clearAuth() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        localStorage.removeItem(CLIENT_KEY);
        localStorage.removeItem(PROFESSIONAL_KEY);
    }

    function getStoredUser() {
        const value = localStorage.getItem(USER_KEY);
        if (!value) {
            return null;
        }
        try {
            return JSON.parse(value);
        } catch {
            return null;
        }
    }

    function storeUser(user) {
        if (user) {
            localStorage.setItem(USER_KEY, JSON.stringify(user));
        }
    }

    window.ApiClient = {
        baseUrl,
        request,
        get,
        post,
        put,
        delete: del,
        buildQuery,
        notify: showNotification,
        getStoredUser,
        storeUser,
        clearAuth,
        TOKEN_KEY,
        USER_KEY,
        CLIENT_KEY,
        PROFESSIONAL_KEY,
        ApiClientError
    };
})(window, document);

