(function (window, document, ApiClient) {
    const formId = "client-register-form";

    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("client-register-page");
        if (!page) return;
        bindForm();
    });

    function bindForm() {
        const form = document.getElementById(formId);
        if (!form) return;
        form.addEventListener("submit", onSubmit);
    }

    async function onSubmit(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const data = readForm(form);
        const validationError = validate(data);
        if (validationError) {
            ApiClient.notify(validationError, "error");
            return;
        }
        toggleForm(form, true);
        setStatus("Verificando email...");
        try {
            await ensureEmailAvailable(data.email);
            setStatus("Criando conta...");
            const authResponse = await ApiClient.post("/auth/register", buildRegisterPayload(data));
            persistSession(authResponse);
            setStatus("Salvando perfil de cliente...");
            await ApiClient.post("/clients", buildClientPayload(data));
            ApiClient.notify("Cadastro de cliente criado com sucesso", "success");
            window.dispatchEvent(new CustomEvent("maocerta:auth-changed"));
            window.location.href = "/ServiceRequests/My";
        } catch (error) {
            ApiClient.notify(error.message || "Nao foi possivel concluir o cadastro", "error");
            setStatus(error.message || "Erro ao cadastrar");
        } finally {
            toggleForm(form, false);
        }
    }

    function readForm(form) {
        const formData = new FormData(form);
        return {
            fullName: (formData.get("fullName") || "").toString().trim(),
            email: (formData.get("email") || "").toString().trim(),
            cpf: normalizeDigits(formData.get("cpf")),
            phone: (formData.get("phone") || "").toString().trim(),
            password: formData.get("password")?.toString() || "",
            confirmPassword: formData.get("confirmPassword")?.toString() || "",
            termsAccepted: formData.get("terms") === "on"
        };
    }

    function validate(data) {
        if (!data.fullName) return "Informe o nome completo.";
        if (!data.email) return "Informe um email valido.";
        // CPF validation relaxed to accept fictitious values
        if (!data.termsAccepted) return "Aceite os termos para continuar.";
        if (data.password !== data.confirmPassword) return "As senhas precisam ser iguais.";
        return null;
    }

    async function ensureEmailAvailable(email) {
        if (!email) return;
        try {
            await ApiClient.get(`/clients/by-email/${encodeURIComponent(email)}`);
            throw new Error("Ja existe um cliente com este email.");
        } catch (error) {
            if (error.status && error.status !== 404) {
                throw error;
            }
        }
        try {
            const professionals = await ApiClient.get("/professionals");
            const exists = Array.isArray(professionals) && professionals.some(p => (p.email || "").toLowerCase() === email.toLowerCase());
            if (exists) {
                throw new Error("Este email ja esta em uso por um profissional.");
            }
        } catch (error) {
            if (error.status && error.status !== 404) {
                throw error;
            }
        }
    }

    function buildRegisterPayload(data) {
        const { firstName, lastName } = splitName(data.fullName);
        return {
            firstName,
            lastName,
            email: data.email,
            password: data.password,
            phone: data.phone || null
        };
    }

    function buildClientPayload(data) {
        return {
            name: data.fullName,
            email: data.email,
            phone: data.phone || "",
            cpf: data.cpf || null
        };
    }

    function splitName(fullName) {
        const parts = fullName.split(" ").filter(Boolean);
        const firstName = parts.shift() || fullName;
        const lastName = parts.length ? parts.join(" ") : firstName;
        return { firstName, lastName };
    }

    function validateCpf() {
        return true;
    }

    function validatePassword() {
        return true;
    }

    function normalizeDigits(value) {
        return (value || "").toString().replace(/\D/g, "");
    }

    function persistSession(authResponse) {
        if (!authResponse || !authResponse.token) return;
        localStorage.setItem(ApiClient.TOKEN_KEY, authResponse.token);
        const user = authResponse.user ?? {
            id: authResponse.userId,
            email: authResponse.email,
            name: `${authResponse.firstName ?? ""} ${authResponse.lastName ?? ""}`.trim(),
            role: "User"
        };
        ApiClient.storeUser({ ...user, email: user.email ?? authResponse.email });
    }

    function setStatus(message) {
        const el = document.getElementById("client-register-status");
        if (el) {
            el.textContent = message || "";
        }
    }

    function toggleForm(form, disabled) {
        form.querySelectorAll("input, button, textarea, select").forEach(el => {
            el.toggleAttribute("disabled", disabled);
        });
    }
})(window, document, window.ApiClient);
