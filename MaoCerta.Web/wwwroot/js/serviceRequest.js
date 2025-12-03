(function (window, document, ApiClient) {
    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("service-request-page");
        if (!page) {
            return;
        }
        hydrateProfessional(page);
        const form = document.getElementById("service-request-form");
        setMinDate(form);
        form?.addEventListener("submit", event => submitRequest(event, page));
    });

    function setMinDate(form) {
        if (!form) return;
        const input = form.querySelector('input[name="scheduledDate"]');
        if (!input) return;
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        input.min = today.toISOString().split("T")[0];
    }

    function hydrateProfessional(page) {
        const professionalId = page.dataset.professionalId;
        const input = document.querySelector('#service-request-form [name="professionalId"]');
        if (professionalId && input && !input.value) {
            input.value = professionalId;
        }
        const params = new URLSearchParams(window.location.search);
        const professionalName = params.get("professionalName") || page.dataset.professionalName;
        if (professionalName) {
            const label = document.getElementById("selected-professional");
            if (label) {
                label.textContent = professionalName;
            }
        }
        hydrateClientInfo();
    }

    async function hydrateClientInfo() {
        const client = await window.Auth?.loadClientProfile?.();
        const target = document.getElementById("selected-client");
        if (!target) return;
        if (!client) {
            target.textContent = "Complete seu cadastro de cliente para enviar solicitações.";
            return;
        }
        const parts = [client.name, client.email, client.phone].filter(Boolean);
        target.textContent = parts.join(" • ");
    }

    async function submitRequest(event, page) {
        event.preventDefault();
        if (!window.Auth?.requireAuth()) {
            return;
        }
        const form = event.currentTarget;
        const formData = new FormData(form);
        const payload = {
            professionalId: Number(formData.get("professionalId")),
            title: formData.get("title"),
            description: formData.get("description"),
            serviceAddress: formData.get("serviceAddress") || null,
            scheduledDate: formData.get("scheduledDate") ? new Date(formData.get("scheduledDate")).toISOString() : null,
            proposedValue: formData.get("proposedValue") ? Number(formData.get("proposedValue")) : null,
            observations: formData.get("observations") || null
        };

        if (!payload.professionalId) {
            ApiClient.notify("Selecione um profissional", "error");
            return;
        }
        if (payload.scheduledDate) {
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const chosen = new Date(payload.scheduledDate);
            if (chosen < today) {
                ApiClient.notify("Escolha uma data a partir de hoje.", "error");
                return;
            }
        }
        const clientProfile = await window.Auth.loadClientProfile();
        if (!clientProfile) {
            ApiClient.notify("Complete seu cadastro de cliente antes de solicitar serviços.", "error");
            return;
        }
        payload.clientId = clientProfile.id;

        try {
            await ApiClient.post("/service-requests", payload);
            ApiClient.notify("Solicitação enviada!", "success");
            form.reset();
            setMinDate(form);
            window.location.href = "/";
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao enviar solicitação", "error");
        }
    }
})(window, document, window.ApiClient);
