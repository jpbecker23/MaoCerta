(function (window, document, ApiClient, Auth) {
    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("professional-register-page");
        if (!page) return;
        if (!Auth.requireAuth()) return;
        preloadUser();
        bindForm();
        loadCategories();
    });

    function bindForm() {
        const form = document.getElementById("professional-register-form");
        if (!form) return;
        form.addEventListener("submit", async event => {
            event.preventDefault();
            if (!Auth.requireAuth()) return;
            const payload = buildPayload(form);
            if (!payload) return;
            setStatus("Enviando...");
            toggleForm(form, true);
            try {
                const result = await ApiClient.post("/professionals", payload);
                ApiClient.notify("Cadastro profissional salvo com sucesso", "success");
                if (result?.id) {
                    window.location.href = `/Professionals/${result.id}`;
                } else {
                    window.location.href = "/Professionals";
                }
            } catch (error) {
                setStatus(error.message || "Não foi possível salvar o cadastro");
                ApiClient.notify(error.message || "Não foi possível salvar o cadastro", "error");
            } finally {
                toggleForm(form, false);
            }
        });
    }

    function buildPayload(form) {
        const data = new FormData(form);
        const categoryId = Number(data.get("categoryId"));
        if (!categoryId) {
            ApiClient.notify("Selecione uma categoria", "error");
            return null;
        }
        return {
            name: data.get("name")?.toString().trim(),
            email: data.get("email")?.toString().trim(),
            phone: data.get("phone")?.toString().trim(),
            address: data.get("address")?.toString().trim(),
            description: (data.get("description") || "").toString().trim(),
            categoryId
        };
    }

    async function loadCategories() {
        const select = document.getElementById("professional-category");
        if (!select) return;
        select.innerHTML = '<option value="">Carregando...</option>';
        try {
            const categories = await ApiClient.get("/categories");
            if (!categories?.length) {
                select.innerHTML = '<option value="">Nenhuma categoria encontrada</option>';
                return;
            }
            select.innerHTML = '<option value="">Selecione uma categoria</option>' + categories
                .map(c => `<option value="${c.id}">${c.name}</option>`)
                .join("\n");
        } catch (error) {
            select.innerHTML = '<option value="">Erro ao carregar categorias</option>';
            ApiClient.notify(error.message || "Erro ao carregar categorias", "error");
        }
    }

    function preloadUser() {
        const user = Auth.getUser ? Auth.getUser() : ApiClient.getStoredUser();
        if (!user) return;
        const form = document.getElementById("professional-register-form");
        if (!form) return;
        const nameInput = form.querySelector('input[name="name"]');
        const emailInput = form.querySelector('input[name="email"]');
        if (nameInput && !nameInput.value) {
            nameInput.value = user.name || "";
        }
        if (emailInput && !emailInput.value && user.email) {
            emailInput.value = user.email;
        }
    }

    function setStatus(message) {
        const el = document.getElementById("professional-register-status");
        if (el) {
            el.textContent = message || "";
        }
    }

    function toggleForm(form, disabled) {
        form.querySelectorAll("input, textarea, select, button").forEach(el => {
            el.toggleAttribute("disabled", disabled);
        });
    }
})(window, document, window.ApiClient, window.Auth);
