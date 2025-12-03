(function (window, document, ApiClient) {
    const pageId = "professional-signup-page";

    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById(pageId);
        if (!page) return;
        bindForm();
        bindCepLookup();
        loadCategories();
    });

    function bindForm() {
        const form = document.getElementById("professional-signup-form");
        if (!form) return;
        form.addEventListener("submit", onSubmit);
    }

    function bindCepLookup() {
        const input = document.querySelector('input[name="postalCode"]');
        if (!input) return;
        input.addEventListener("blur", async () => {
            const cep = normalizeDigits(input.value);
            if (cep.length !== 8) return;
            try {
                const result = await fetchViaCep(cep);
                if (result) {
                    setValue('street', result.logradouro);
                    setValue('neighborhood', result.bairro);
                    setValue('city', result.localidade);
                    setValue('state', result.uf);
                }
            } catch {
                // Ignore lookup errors
            }
        });
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
            setStatus("Salvando perfil profissional...");
            const professional = await ApiClient.post("/professionals", buildProfessionalPayload(data));
            if (professional) {
                localStorage.setItem(ApiClient.PROFESSIONAL_KEY || "maocerta.professional", JSON.stringify(professional));
            }
            ApiClient.notify("Cadastro profissional criado com sucesso", "success");
            window.dispatchEvent(new CustomEvent("maocerta:auth-changed"));
            if (professional?.id) {
                window.location.href = `/Professionals/${professional.id}`;
            } else {
                window.location.href = "/Professionals";
            }
        } catch (error) {
            ApiClient.notify(error.message || "Nao foi possivel concluir o cadastro", "error");
            setStatus(error.message || "Erro ao cadastrar");
        } finally {
            toggleForm(form, false);
        }
    }

    async function ensureEmailAvailable(email) {
        if (!email) return;
        try {
            await ApiClient.get(`/clients/by-email/${encodeURIComponent(email)}`);
            throw new Error("Este email ja esta em uso por um cliente.");
        } catch (error) {
            if (error.status && error.status !== 404) {
                throw error;
            }
        }
        const professionals = await ApiClient.get("/professionals");
        const exists = Array.isArray(professionals) && professionals.some(p => (p.email || "").toLowerCase() === email.toLowerCase());
        if (exists) {
            throw new Error("Este email ja esta em uso por um profissional.");
        }
    }

    function readForm(form) {
        const fd = new FormData(form);
        return {
            fullName: (fd.get("fullName") || "").toString().trim(),
            email: (fd.get("email") || "").toString().trim(),
            cpf: normalizeDigits(fd.get("cpf")),
            phone: (fd.get("phone") || "").toString().trim(),
            postalCode: normalizeDigits(fd.get("postalCode")),
            state: (fd.get("state") || "").toString().trim().toUpperCase(),
            city: (fd.get("city") || "").toString().trim(),
            neighborhood: (fd.get("neighborhood") || "").toString().trim(),
            street: (fd.get("street") || "").toString().trim(),
            number: (fd.get("number") || "").toString().trim(),
            complement: (fd.get("complement") || "").toString().trim(),
            specialization: (fd.get("specialization") || "").toString().trim(),
            categoryId: Number(fd.get("categoryId")),
            password: fd.get("password")?.toString() || "",
            confirmPassword: fd.get("confirmPassword")?.toString() || "",
            termsAccepted: fd.get("terms") === "on"
        };
    }

    function validate(data) {
        if (!data.fullName) return "Informe o nome completo.";
        if (!data.email) return "Informe um email valido.";
        // CPF validation relaxada
        if (!validatePhone(data.phone)) return "Informe um telefone valido.";
        if (!data.categoryId) return "Escolha uma categoria.";
        if (!data.postalCode || data.postalCode.length !== 8) return "CEP invalido.";
        if (!data.street || !data.number || !data.neighborhood || !data.city || !data.state) return "Endereco incompleto.";
        if (!data.specialization) return "Descreva sua especializacao.";
        if (data.password !== data.confirmPassword) return "As senhas precisam ser iguais.";
        if (!data.termsAccepted) return "Aceite os termos para continuar.";
        return null;
    }

    function buildRegisterPayload(data) {
        const { firstName, lastName } = splitName(data.fullName);
        return {
            firstName,
            lastName,
            email: data.email,
            password: data.password,
            phone: data.phone
        };
    }

    function buildProfessionalPayload(data) {
        const address = [
            `${data.street}, ${data.number}`,
            data.complement ? data.complement : null,
            data.neighborhood ? `Bairro ${data.neighborhood}` : null,
            `${data.city}/${data.state}`,
            data.postalCode ? `CEP ${formatCep(data.postalCode)}` : null
        ].filter(Boolean).join(" - ");

        return {
            name: data.fullName,
            email: data.email,
            phone: data.phone,
            address,
            description: data.specialization,
            categoryId: data.categoryId,
            cpf: data.cpf,
            postalCode: data.postalCode,
            city: data.city,
            state: data.state,
            neighborhood: data.neighborhood
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

    function validatePhone(phone) {
        if (!phone) return false;
        const cleaned = normalizeDigits(phone);
        return cleaned.length >= 10 && cleaned.length <= 11;
    }

    function validatePassword() {
        return true;
    }

    function normalizeDigits(value) {
        return (value || "").toString().replace(/\D/g, "");
    }

    function formatCep(value) {
        const digits = normalizeDigits(value);
        if (digits.length !== 8) return value;
        return `${digits.substring(0, 5)}-${digits.substring(5)}`;
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
        const el = document.getElementById("professional-register-status");
        if (el) {
            el.textContent = message || "";
        }
    }

    function toggleForm(form, disabled) {
        form.querySelectorAll("input, button, textarea, select").forEach(el => {
            el.toggleAttribute("disabled", disabled);
        });
    }

    async function loadCategories() {
        const select = document.getElementById("signup-category");
        if (!select) return;
        select.innerHTML = '<option value="">Carregando...</option>';
        try {
            const categories = await ApiClient.get("/categories");
            if (!categories?.length) {
                select.innerHTML = '<option value="">Nenhuma categoria encontrada</option>';
                return;
            }
            select.innerHTML = '<option value="">Selecione</option>' + categories.map(c => `<option value="${c.id}">${c.name}</option>`).join("");
        } catch (error) {
            select.innerHTML = '<option value="">Erro ao carregar</option>';
            ApiClient.notify(error.message || "Erro ao carregar categorias", "error");
        }
    }

    async function fetchViaCep(cep) {
        const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        if (!response.ok) {
            throw new Error("CEP nao encontrado");
        }
        const data = await response.json();
        if (data.erro) {
            throw new Error("CEP nao encontrado");
        }
        return data;
    }

    function setValue(name, value) {
        if (!value) return;
        const input = document.querySelector(`[name="${name}"]`);
        if (input && !input.value) {
            input.value = value;
        }
    }
})(window, document, window.ApiClient);
