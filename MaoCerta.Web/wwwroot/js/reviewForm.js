(function (window, document, ApiClient) {
    const reviewState = {
        adminReviews: []
    };

    document.addEventListener("DOMContentLoaded", () => {
        setupStandaloneForm();
        setupEmbeddedForm();
        setupAdminDashboard();
    });

    window.addEventListener("maocerta:professional-detail", event => {
        const professionalId = event.detail?.id;
        renderEmbeddedForm(professionalId);
    });

    function setupStandaloneForm() {
        const form = document.getElementById("review-form");
        if (!form) return;
        attachRangeHandlers(form);
        form.addEventListener("submit", event => handleSubmit(event, form));
    }

    function setupEmbeddedForm() {
        const container = document.getElementById("embedded-review-form");
        if (container && !container.querySelector("form")) {
            renderEmbeddedForm(Number(container.closest('#professional-detail')?.dataset.professionalId));
        }
    }

    async function handleSubmit(event, form) {
        event.preventDefault();
        if (!window.Auth?.requireAuth()) {
            return;
        }
        const clientProfile = await window.Auth.loadClientProfile();
        if (!clientProfile) {
            ApiClient.notify("Finalize seu cadastro de cliente para avaliar.", "error");
            return;
        }
        const data = new FormData(form);
        const payload = {
            clientId: clientProfile.id,
            professionalId: Number(data.get("professionalId")),
            serviceRequestId: Number(data.get("serviceRequestId")),
            priceRating: Number(data.get("priceRating")),
            qualityRating: Number(data.get("qualityRating")),
            speedRating: Number(data.get("speedRating")),
            communicationRating: Number(data.get("communicationRating")),
            professionalismRating: Number(data.get("professionalismRating")),
            comment: data.get("comment") || null,
            positivePoints: data.get("positivePoints") || null,
            negativePoints: data.get("negativePoints") || null
        };
        if (!payload.professionalId || !payload.serviceRequestId) {
            ApiClient.notify("Informe os identificadores do serviço.", "error");
            return;
        }
        try {
            await ApiClient.post("/reviews", payload);
            ApiClient.notify("Avaliação registrada com sucesso!", "success");
            form.reset();
            attachRangeHandlers(form);
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao enviar avaliação", "error");
        }
    }

    function attachRangeHandlers(form) {
        form.querySelectorAll('input[type="range"]').forEach(range => {
            const label = range.nextElementSibling;
            if (label) {
                label.textContent = range.value;
            }
            range.addEventListener("input", () => {
                if (label) {
                    label.textContent = range.value;
                }
            });
        });
    }

    function renderEmbeddedForm(professionalId) {
        const container = document.getElementById("embedded-review-form");
        if (!container) return;
        container.innerHTML = getFormMarkup(professionalId);
        const form = container.querySelector("form");
        attachRangeHandlers(form);
        form.addEventListener("submit", event => handleSubmit(event, form));
    }

    function getFormMarkup(professionalId) {
        const id = professionalId ?? "";
        return `<form class="embedded-review-form">
            <input type="hidden" name="professionalId" value="${id}" />
            <label>Solicitação (ID)
                <input type="number" name="serviceRequestId" required />
            </label>
            <div class="criteria-form">
                ${criteriaInput("priceRating", "Preço justo")}
                ${criteriaInput("qualityRating", "Qualidade")}
                ${criteriaInput("speedRating", "Pontualidade")}
                ${criteriaInput("communicationRating", "Comunicação")}
                ${criteriaInput("professionalismRating", "Profissionalismo")}
            </div>
            <label>Comentário<textarea name="comment" rows="3"></textarea></label>
            <label>Pontos positivos<textarea name="positivePoints" rows="2"></textarea></label>
            <label>Pontos negativos<textarea name="negativePoints" rows="2"></textarea></label>
            <button class="btn primary" type="submit">Enviar avaliação</button>
        </form>`;
    }

    function criteriaInput(name, label) {
        return `<label>${label}
            <input type="range" min="1" max="5" step="1" value="5" name="${name}" />
            <span class="criteria-value">5</span>
        </label>`;
    }

    function setupAdminDashboard() {
        const dashboard = document.getElementById("admin-review-dashboard");
        if (!dashboard) return;
        if (!window.Auth?.requireAuth()) {
            return;
        }
        document.getElementById("admin-filter-status")?.addEventListener("change", renderAdminReviews);
        document.getElementById("admin-filter-rating")?.addEventListener("change", renderAdminReviews);
        const list = document.getElementById("admin-review-list");
        list?.addEventListener("click", handleAdminAction);
        loadAdminReviews();
    }

    async function loadAdminReviews() {
        try {
            const reviews = await ApiClient.get("/reviews");
            reviewState.adminReviews = reviews ?? [];
            renderAdminReviews();
        } catch (error) {
            const list = document.getElementById("admin-review-list");
            if (list) {
                list.innerHTML = `<p class="muted">${error.message}</p>`;
            }
        }
    }

    function renderAdminReviews() {
        const list = document.getElementById("admin-review-list");
        if (!list) return;
        const status = document.getElementById("admin-filter-status")?.value ?? "all";
        const minRating = parseFloat(document.getElementById("admin-filter-rating")?.value ?? "0");
        const filtered = reviewState.adminReviews.filter(review => {
            if (status === "active" && review.isActive === false) return false;
            if (status === "inactive" && review.isActive !== false) return false;
            if ((review.overallRating ?? 0) < minRating) return false;
            return true;
        });
        if (!filtered.length) {
            list.innerHTML = `<p class="muted">${list.dataset.emptyMessage}</p>`;
            return;
        }
        list.innerHTML = filtered.map(review => `
            <article class="admin-review-item" data-id="${review.id}">
                <header>
                    <strong>${review.professionalName}</strong>
                    <span>${new Date(review.createdAt).toLocaleDateString()}</span>
                </header>
                <p>${review.comment ?? "Sem comentário"}</p>
                <small>${review.clientName} · Nota ${(review.overallRating ?? 0).toFixed(1)}</small>
                <footer>
                    <div class="labels">
                        <span class="badge">${review.isActive === false ? "Inativo" : "Ativo"}</span>
                    </div>
                    <div class="actions">
                        <button class="btn ghost" type="button" data-action="remove">Arquivar</button>
                    </div>
                </footer>
            </article>`).join("\n");
    }

    async function handleAdminAction(event) {
        const button = event.target.closest('button[data-action]');
        if (!button) return;
        const reviewEl = button.closest('.admin-review-item');
        const id = reviewEl?.dataset.id;
        if (!id) return;
        if (button.dataset.action === "remove") {
            try {
                await ApiClient.delete(`/reviews/${id}`);
                ApiClient.notify("Avaliação removida", "success");
                reviewState.adminReviews = reviewState.adminReviews.filter(review => review.id !== Number(id));
                renderAdminReviews();
            } catch (error) {
                ApiClient.notify(error.message || "Não foi possível remover", "error");
            }
        }
    }

    window.ReviewForm = {
        renderEmbeddedForm
    };
})(window, document, window.ApiClient);

