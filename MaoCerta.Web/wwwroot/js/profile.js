(function (window, document, ApiClient, Auth) {
    const state = {
        client: null,
        professional: null,
        professionalDetail: null
    };

    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("profile-page");
        if (!page) return;
        if (!Auth.requireAuth()) return;
        loadData();
        bindForms();
    });

    async function loadData() {
        await Promise.all([loadClient(), loadProfessional()]);
        hydrateContactForm();
    }

    async function loadClient() {
        try {
            state.client = await Auth.loadClientProfile?.(true);
        } catch {
            state.client = null;
        }
    }

    async function loadProfessional() {
        const user = Auth.getUser?.() || ApiClient.getStoredUser?.();
        const email = (user?.email || "").toLowerCase();
        if (!email) return;
        try {
            const professionals = await ApiClient.get("/professionals");
            state.professional = professionals.find(p => (p.email || "").toLowerCase() === email) || null;
            if (state.professional?.id) {
                state.professionalDetail = await ApiClient.get(`/professionals/${state.professional.id}`);
            }
        } catch {
            state.professional = null;
        }
    }

    function hydrateContactForm() {
        const form = document.getElementById("contact-form");
        if (!form) return;
        const client = state.client;
        const professional = state.professionalDetail || state.professional;
        const target = client || professional;
        if (!target) return;
        setValue(form, "name", target.name || "");
        setValue(form, "email", target.email || "");
        setValue(form, "phone", target.phone || "");
    }

    function bindForms() {
        const contactForm = document.getElementById("contact-form");
        if (contactForm) {
            contactForm.addEventListener("submit", handleContactSubmit);
        }
        const passwordForm = document.getElementById("password-form");
        if (passwordForm) {
            passwordForm.addEventListener("submit", handlePasswordSubmit);
        }
    }

    async function handleContactSubmit(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const data = readForm(form);
        setStatus("contact-status", "Salvando...");
        try {
            if (state.client) {
                await ApiClient.put(`/clients/${state.client.id}`, {
                    id: state.client.id,
                    name: data.name || state.client.name,
                    email: data.email || state.client.email,
                    phone: data.phone || state.client.phone,
                    address: data.address || state.client.address,
                    age: state.client.age ?? null
                });
                ApiClient.notify("Dados de cliente atualizados", "success");
                state.client = await Auth.loadClientProfile?.(true);
            }
            if (state.professional) {
                const current = state.professionalDetail || state.professional;
                const payload = {
                    id: current.id,
                    name: data.name || current.name,
                    email: data.email || current.email,
                    phone: data.phone || current.phone,
                    address: current.address,
                    description: current.description,
                    categoryId: current.categoryId
                };
                await ApiClient.put(`/professionals/${current.id}`, payload);
                ApiClient.notify("Dados de profissional atualizados", "success");
                state.professionalDetail = await ApiClient.get(`/professionals/${current.id}`);
            }
            setStatus("contact-status", "Dados salvos");
        } catch (error) {
            setStatus("contact-status", error.message || "Erro ao salvar");
            ApiClient.notify(error.message || "Erro ao salvar contato", "error");
        }
    }

    async function handlePasswordSubmit(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const data = readForm(form);
        if (!data.currentPassword || !data.newPassword || !data.confirmPassword) {
            ApiClient.notify("Preencha todas as senhas", "error");
            return;
        }
        if (data.newPassword !== data.confirmPassword) {
            ApiClient.notify("As senhas novas nao coincidem", "error");
            return;
        }
        setStatus("password-status", "Atualizando senha...");
        try {
            await ApiClient.post("/auth/change-password", {
                currentPassword: data.currentPassword,
                newPassword: data.newPassword
            });
            ApiClient.notify("Senha atualizada", "success");
            form.reset();
            setStatus("password-status", "Senha alterada");
        } catch (error) {
            setStatus("password-status", error.message || "Erro ao alterar senha");
            ApiClient.notify(error.message || "Erro ao alterar senha", "error");
        }
    }

    function readForm(form) {
        const fd = new FormData(form);
        const result = {};
        for (const [key, value] of fd.entries()) {
            result[key] = value.toString().trim();
        }
        return result;
    }

    function setValue(form, name, value) {
        const input = form.querySelector(`[name="${name}"]`);
        if (input && !input.value) {
            input.value = value ?? "";
        }
    }

    function setStatus(elementId, text) {
        const el = document.getElementById(elementId);
        if (el) el.textContent = text || "";
    }

    function validatePassword(password) {
        if (!password || password.length < 8) return false;
        const hasUpper = /[A-Z]/.test(password);
        const hasLower = /[a-z]/.test(password);
        const hasNumber = /\d/.test(password);
        const hasSymbol = /[^A-Za-z0-9]/.test(password);
        return hasUpper && hasLower && hasNumber && hasSymbol;
    }
})(window, document, window.ApiClient, window.Auth);
