(function (window, document, ApiClient, Auth) {
    const STATUS_LABELS = {
        0: "Pendente",
        1: "Aceita",
        2: "Recusada",
        3: "Em andamento",
        4: "Concluida",
        5: "Cancelada"
    };

    const STATUS = {
        Pending: 0,
        Accepted: 1,
        Rejected: 2,
        InProgress: 3,
        Completed: 4,
        Cancelled: 5
    };

    const PROFESSIONAL_KEY = ApiClient?.PROFESSIONAL_KEY || "maocerta.professional";

    const state = {
        professional: null,
        requests: [],
        averageRating: null,
        reviewsByRequest: new Map()
    };

    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("inbox-page");
        if (!page) return;
        init();
    });

    async function init() {
        if (!Auth?.requireAuth?.()) return;
        const professional = await loadProfessionalProfile();
        const professionalName = document.getElementById("inbox-professional-name");
        if (professionalName) {
            professionalName.textContent = professional ? professional.name || professional.email : "";
        }
        if (!professional) {
            setMessage("Complete seu cadastro de profissional para visualizar as solicitacoes recebidas.");
            clearList();
            updateSummary();
            return;
        }
        state.professional = professional;
        await loadAverageRating();
        await loadReviewsByRequest();
        await loadRequests();
    }

    async function loadReviewsByRequest() {
        state.reviewsByRequest.clear();
        const ids = state.requests.map(r => r.id);
        await Promise.all(ids.map(async id => {
            try {
                const review = await ApiClient.get(`/reviews/service-request/${id}`);
                if (review) {
                    state.reviewsByRequest.set(id, review);
                }
            } catch {
                // ignore missing reviews
            }
        }));
    }

    async function loadAverageRating() {
        if (!state.professional?.id) return;
        try {
            const response = await ApiClient.get(`/reviews/professional/${state.professional.id}/average-rating`);
            state.averageRating = response?.averageRating ?? null;
        } catch {
            state.averageRating = null;
        }
    }

    function clearList() {
        const list = document.getElementById("inbox-list");
        if (list) {
            list.innerHTML = "";
        }
    }

    function setMessage(text) {
        const message = document.getElementById("inbox-message");
        if (message) {
            message.textContent = text || "";
        }
    }

    function readCachedProfessional() {
        const value = localStorage.getItem(PROFESSIONAL_KEY);
        if (!value) return null;
        try {
            return JSON.parse(value);
        } catch {
            localStorage.removeItem(PROFESSIONAL_KEY);
            return null;
        }
    }

    function cacheProfessional(profile) {
        if (!profile) return;
        localStorage.setItem(PROFESSIONAL_KEY, JSON.stringify(profile));
    }

    async function loadProfessionalProfile() {
        const cached = readCachedProfessional();
        if (cached) return cached;

        const user = Auth?.getUser?.() || ApiClient.getStoredUser?.();
        if (!user?.email) {
            return null;
        }
        try {
            const professionals = await ApiClient.get("/professionals");
            const profile = professionals.find(p => (p.email || "").toLowerCase() === user.email.toLowerCase()) || null;
            if (profile) {
                cacheProfessional(profile);
            }
            return profile;
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao carregar perfil do profissional", "error");
            return null;
        }
    }

    async function loadRequests() {
        const list = document.getElementById("inbox-list");
        if (!list || !state.professional) return;
        list.innerHTML = '<p class="muted">Carregando...</p>';
        setMessage("");
        try {
            const data = await ApiClient.get(`/service-requests/professional/${state.professional.id}`);
            state.requests = data || [];
            updateSummary();
            if (!state.requests.length) {
                list.innerHTML = `<p class="muted">${list.dataset.emptyMessage}</p>`;
                return;
            }
            list.innerHTML = state.requests.map(renderCard).join("\n");
            bindActions(list);
        } catch (error) {
            list.innerHTML = `<p class="muted">${error.message || "Erro ao carregar solicitacoes"}</p>`;
        }
    }

    function updateSummary() {
        const summary = document.getElementById("inbox-summary");
        if (!summary) return;
        const total = state.requests.length;
        const pending = state.requests.filter(r => r.status === STATUS.Pending).length;
        const inProgress = state.requests.filter(r => r.status === STATUS.InProgress).length;
        summary.textContent = total
            ? `${total} solicitacoes - ${pending} pendentes - ${inProgress} em andamento`
            : "Nenhuma solicitacao";
    }

    function renderCard(req) {
        const status = STATUS_LABELS[req.status] || "Desconhecido";
        const actions = renderActions(req);
        const verification = req.verificationCode
            ? `<small class="muted">Codigo de verificacao: <strong>${req.verificationCode}</strong></small>`
            : "";
        const review = state.reviewsByRequest.get(req.id);
        const ratingInfo = req.status === STATUS.Completed && review
            ? `<small class="muted">Nota do cliente: <strong>${Number(review.overallRating || 0).toFixed(1)}</strong></small>`
            : "";
        return `<article class="card" data-id="${req.id}">
            <header class="section-header">
                <div>
                    <h3>${req.title}</h3>
                    <p class="muted">Cliente: ${req.clientName}</p>
                </div>
                <span class="badge">${status}</span>
            </header>
            <p>${req.description ?? ""}</p>
            <small class="muted">Local: ${req.serviceAddress ?? "Nao informado"}</small><br />
            <small class="muted">Valor proposto: ${req.proposedValue ? `R$ ${req.proposedValue}` : "Nao informado"}</small><br />
            <small class="muted">Data: ${formatDate(req.scheduledDate)}</small><br />
            <small class="muted">Observacoes: ${req.observations ?? "Nao informado"}</small><br />
            ${verification}
            ${ratingInfo}
            <footer class="actions">
                ${actions || '<small class="muted">Sem acoes disponiveis</small>'}
            </footer>
        </article>`;
    }

    function formatDate(value) {
        if (!value) return "Nao informado";
        try {
            return new Date(value).toLocaleDateString("pt-BR");
        } catch {
            return "Nao informado";
        }
    }

    function renderActions(req) {
        if (req.status === STATUS.Pending) {
            return `
                <button class="btn primary" data-action="accept">Aceitar</button>
                <button class="btn ghost" data-action="reject">Recusar</button>`;
        }
        if (req.status === STATUS.Accepted) {
            return `
                <button class="btn primary" data-action="start">Iniciar</button>
                <button class="btn ghost" data-action="cancel">Cancelar</button>`;
        }
        if (req.status === STATUS.InProgress) {
            return `
                <button class="btn primary" data-action="complete">Concluir</button>
                <button class="btn ghost" data-action="generate">Gerar codigo</button>`;
        }
        return "";
    }

    function bindActions(list) {
        list.querySelectorAll("[data-action]").forEach(btn => {
            btn.addEventListener("click", async event => {
                const card = event.currentTarget.closest("[data-id]");
                const id = Number(card?.dataset.id);
                const action = event.currentTarget.dataset.action;
                if (!id || !action) return;
                await handleAction(action, id);
            });
        });
    }

    async function handleAction(action, id) {
        const statusMap = {
            accept: STATUS.Accepted,
            reject: STATUS.Rejected,
            start: STATUS.InProgress,
            cancel: STATUS.Cancelled
        };
        if (action === "generate") {
            await handleGenerateCode(id);
            await loadRequests();
            return;
        }
        if (action === "complete") {
            await handleComplete(id);
            return;
        }
        const status = statusMap[action];
        if (status === undefined) return;
        const updated = await updateStatus(id, status);
        if (status === STATUS.Accepted && updated?.verificationCode) {
            ApiClient.notify(`Codigo de verificacao gerado: ${updated.verificationCode}`, "info");
            try {
                await navigator.clipboard?.writeText?.(updated.verificationCode);
            } catch {
                // clipboard may not be available
            }
        }
        await loadRequests();
    }

    async function handleGenerateCode(id) {
        try {
            const response = await ApiClient.post(`/service-requests/${id}/generate-verification-code`, {});
            const code = response?.verificationCode || response?.code || response;
            if (code) {
                ApiClient.notify(`Codigo: ${code}`, "info");
                try {
                    await navigator.clipboard?.writeText?.(code);
                } catch {
                    // clipboard may not be available; ignore
                }
            }
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao gerar codigo", "error");
        }
    }

    async function handleComplete(id) {
        const code = window.prompt("Digite o codigo informado pelo cliente para concluir o servico:", "");
        if (!code || !code.trim()) {
            ApiClient.notify("Codigo de confirmacao obrigatorio para concluir.", "error");
            return;
        }
        try {
            await updateStatus(id, STATUS.Completed, code.trim());
            ApiClient.notify("Solicitacao marcada como concluida", "success");
            await loadRequests();
        } catch {
            // erro ja tratado por updateStatus
        }
    }

    async function updateStatus(id, status, verificationCode) {
        try {
            const payload = { id, status };
            if (verificationCode) {
                payload.verificationCode = verificationCode;
            }
            return await ApiClient.put(`/service-requests/${id}/status`, payload);
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao atualizar status", "error");
            throw error;
        }
    }
})(window, document, window.ApiClient, window.Auth);

