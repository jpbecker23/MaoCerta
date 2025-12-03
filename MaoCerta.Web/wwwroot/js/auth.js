(function (window, document, ApiClient) {
    if (!ApiClient) {
        throw new Error("ApiClient not found");
    }

    const state = {
        user: ApiClient.getStoredUser()
    };

    const selectors = {
        authNav: () => document.getElementById("auth-nav"),
        loginLink: () => document.getElementById("login-link"),
        registerLink: () => document.getElementById("register-link"),
        logoutButton: () => document.getElementById("logout-button"),
        userPill: () => document.getElementById("auth-user-pill")
    };

    let professionalProfilePromise = null;
    let clientProfilePromise = null;

    function persistSession(authResponse) {
        if (!authResponse || !authResponse.token) {
            throw new Error("Resposta inválida");
        }
        localStorage.setItem(ApiClient.TOKEN_KEY, authResponse.token);
        const userFromResponse = authResponse.user ?? {
            id: authResponse.userId,
            email: authResponse.email,
            name: `${authResponse.firstName ?? ""} ${authResponse.lastName ?? ""}`.trim(),
            role: "User"
        };
        ApiClient.storeUser({ ...userFromResponse, email: userFromResponse.email ?? authResponse.email });
        state.user = ApiClient.getStoredUser();
        ApiClient.notify("Sessão iniciada com sucesso", "success");
        updateNavigation();
    }

    function clearSession() {
        ApiClient.clearAuth();
        state.user = null;
        professionalProfilePromise = null;
        clientProfilePromise = null;
        updateNavigation();
    }

    async function handleLogin(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const payload = getFormData(form);
        if (!payload.email || !payload.password) {
            ApiClient.notify("Informe email e senha", "error");
            return;
        }
        try {
            const response = await ApiClient.post("/auth/login", payload);
            persistSession(response);
            redirectAfterLogin(payload.ReturnUrl);
        } catch (error) {
            ApiClient.notify(error.message || "Falha no login", "error");
        }
    }

    async function handleRegister(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const payload = getFormData(form);
        if (payload.password !== payload.confirmPassword) {
            ApiClient.notify("As senhas precisam ser iguais", "error");
            return;
        }
        try {
            const response = await ApiClient.post("/auth/register", payload);
            persistSession(response);
            redirectAfterLogin();
        } catch (error) {
            ApiClient.notify(error.message || "Não foi possível criar a conta", "error");
        }
    }

    function redirectAfterLogin(returnUrl) {
        if (returnUrl && typeof returnUrl === "string" && returnUrl.startsWith("/")) {
            window.location.href = returnUrl;
            return;
        }
        if (window.location.pathname.toLowerCase().includes("account")) {
            window.location.href = "/";
        } else {
            window.dispatchEvent(new CustomEvent("maocerta:auth-changed"));
        }
    }

    async function handleLogout() {
        if (!localStorage.getItem(ApiClient.TOKEN_KEY)) {
            clearSession();
            return;
        }

        try {
            await ApiClient.post("/auth/logout", {});
        } catch {
            // Mesmo se falhar, limpamos o token local
        }
        ApiClient.notify("Sessão encerrada", "info");
        clearSession();
        window.location.href = "/";
    }

    function getFormData(form) {
        const formData = new FormData(form);
        const payload = {};
        for (const [key, value] of formData.entries()) {
            payload[key] = value.toString().trim();
        }
        return payload;
    }

    function updateNavigation() {
        const user = state.user;
        const loginLink = selectors.loginLink();
        const registerLink = selectors.registerLink();
        const logoutButton = selectors.logoutButton();
        const userPill = selectors.userPill();
        const nav = selectors.authNav();
        if (!nav) {
            return;
        }

        if (user) {
            if (loginLink) loginLink.hidden = true;
            if (registerLink) registerLink.hidden = true;
            if (logoutButton) logoutButton.hidden = false;
            if (userPill) {
                userPill.hidden = false;
                userPill.textContent = user.name ?? user.email;
            }
            nav.dataset.authenticated = "true";
        } else {
            if (loginLink) loginLink.hidden = false;
            if (registerLink) registerLink.hidden = false;
            if (logoutButton) logoutButton.hidden = true;
            if (userPill) userPill.hidden = true;
            nav.dataset.authenticated = "false";
        }

        document.querySelectorAll('[data-role]').forEach(el => {
            const requiredRole = (el.getAttribute('data-role') || '').toLowerCase();
            const userRole = (user?.role || '').toLowerCase();
            el.toggleAttribute('hidden', !user || (requiredRole && requiredRole !== userRole));
        });
        updateProfessionalLinks();
        updateClientLinks();
    }

    function bindForms() {
        const loginForm = document.getElementById("login-form");
        if (loginForm) {
            loginForm.addEventListener("submit", handleLogin);
        }
        const registerForm = document.getElementById("register-form");
        if (registerForm) {
            registerForm.addEventListener("submit", handleRegister);
            registerForm.querySelectorAll('input[type="range"]').forEach(updateRangeLabel);
        }
    }

    function updateRangeLabel(input) {
        const label = input.nextElementSibling;
        if (label) {
            label.textContent = input.value;
        }
        input.addEventListener("input", () => {
            if (label) {
                label.textContent = input.value;
            }
        });
    }

    async function hydrateSession() {
        const token = localStorage.getItem(ApiClient.TOKEN_KEY);
        if (!token) {
            clearSession();
            return;
        }
        if (state.user) {
            updateNavigation();
        }
    }

    function guardPrivateLinks() {
        document.querySelectorAll('[data-requires-auth="true"]').forEach(link => {
            link.addEventListener('click', evt => {
                if (!state.user) {
                    evt.preventDefault();
                    ApiClient.notify("Faça login para continuar", "error");
                    const destination = link.getAttribute('href') || link.href || '/';
                    window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(new URL(destination, window.location.origin).pathname)}`;
                }
            });
        });
    }

    function updateProfessionalLinks() {
        const links = document.querySelectorAll('[data-professional-only="true"]');
        if (!links.length) return;
        const user = state.user || ApiClient.getStoredUser();
        if (!user?.email) {
            links.forEach(link => link.hidden = true);
            professionalProfilePromise = null;
            return;
        }
        if (!professionalProfilePromise) {
            professionalProfilePromise = resolveProfessionalProfile(user.email).catch(() => null);
        }
        professionalProfilePromise.then(profile => {
            links.forEach(link => link.hidden = !profile);
        });
    }

    function updateClientLinks() {
        const links = document.querySelectorAll('[data-client-only="true"]');
        if (!links.length) return;
        const user = state.user || ApiClient.getStoredUser();
        if (!user?.email) {
            links.forEach(link => link.hidden = true);
            clientProfilePromise = null;
            return;
        }
        if (!clientProfilePromise) {
            clientProfilePromise = loadClientProfile().catch(() => null);
        }
        clientProfilePromise.then(profile => {
            links.forEach(link => link.hidden = !profile);
        });
    }

    async function resolveProfessionalProfile(email) {
        if (!email) return null;
        try {
            const professionals = await ApiClient.get("/professionals");
            return professionals.find(p => (p.email || "").toLowerCase() === email.toLowerCase()) || null;
        } catch {
            return null;
        }
    }

    async function loadClientProfile(force = false) {
        if (!force) {
            const cached = localStorage.getItem(ApiClient.CLIENT_KEY);
            if (cached) {
                try {
                    return JSON.parse(cached);
                } catch {
                    localStorage.removeItem(ApiClient.CLIENT_KEY);
                }
            }
        }
        const user = state.user || ApiClient.getStoredUser();
        if (!user?.email) {
            return null;
        }
        try {
            const profile = await ApiClient.get(`/clients/by-email/${encodeURIComponent(user.email)}`);
            localStorage.setItem(ApiClient.CLIENT_KEY, JSON.stringify(profile));
            return profile;
        } catch (error) {
            if (error.status === 404) {
                return null;
            }
            throw error;
        }
    }

    function requireAuth() {
        if (!state.user) {
            ApiClient.notify("Sua sessão expirou", "error");
            window.location.href = "/Account/Login";
            return false;
        }
        return true;
    }

    document.addEventListener("DOMContentLoaded", () => {
        bindForms();
        updateNavigation();
        hydrateSession();
        guardPrivateLinks();
        const logoutButton = selectors.logoutButton();
        if (logoutButton) {
            logoutButton.addEventListener("click", handleLogout);
        }
    });

    window.addEventListener("maocerta:unauthorized", () => {
        ApiClient.notify("Sessão expirada, faça login novamente", "error");
        clearSession();
        if (!window.location.pathname.includes("/Account/Login")) {
            window.location.href = "/Account/Login";
        }
    });

    window.Auth = {
        getUser: () => state.user,
        requireAuth,
        loadClientProfile,
        logout: handleLogout,
        clearSession
    };
})(window, document, window.ApiClient);

