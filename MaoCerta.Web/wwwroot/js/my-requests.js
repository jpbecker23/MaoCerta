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

    const state = {
        requests: [],
        client: null
    };

    document.addEventListener("DOMContentLoaded", () => {
        const page = document.getElementById("my-requests-page");
        if (!page) return;
        init();
    });

    async function init() {
        if (!Auth.requireAuth()) return;
        state.client = await Auth.loadClientProfile(true);
        if (!state.client) {
            ApiClient.notify("Complete seu cadastro de cliente para enviar solicitacoes.", "error");
            return;
        }
        await loadRequests();
    }

    async function loadRequests() {
        const list = document.getElementById("my-requests-list");
        if (!list) return;
        list.innerHTML = '<p class="muted">Carregando...</p>';
        try {
            const data = await ApiClient.get(`/service-requests/client/${state.client.id}`);
            state.requests = data || [];
            if (!state.requests.length) {
                list.innerHTML = `<p class="muted">${list.dataset.emptyMessage}</p>`;
                return;
            }
            list.innerHTML = state.requests.map(renderCard).join("\n");
            bindActions(list);
        } catch (error) {
            list.innerHTML = `<p class="muted">${error.message}</p>`;
        }
    }

    function renderCard(req) {
        const status = STATUS_LABELS[req.status] || "Desconhecido";
        const verification = req.verificationCode
            ? `<small class="muted">Codigo de confirmacao: <strong>${req.verificationCode}</strong></small><br />`
            : "";
        const alreadyRated = hasRated(req.id);
        return `<article class="card" data-id="${req.id}" data-professional-id="${req.professionalId}">
            <header class="section-header">
                <div>
                    <h3>${req.title}</h3>
                    <p class="muted">Profissional: ${req.professionalName}</p>
                </div>
                <span class="badge">${status}</span>
            </header>
            <p>${req.description ?? ""}</p>
            <small class="muted">Local: ${req.serviceAddress ?? "Nao informado"}</small><br />
            <small class="muted">Valor proposto: ${req.proposedValue ? `R$ ${req.proposedValue}` : "Nao informado"}</small><br />
            <small class="muted">Data: ${req.scheduledDate ? new Date(req.scheduledDate).toLocaleDateString("pt-BR") : "Nao informado"}</small><br />
            <small class="muted">Informacoes adicionais: ${req.observations ?? "Nao informado"}</small><br />
            ${verification}
            <footer class="actions">
                <button class="btn ghost" data-action="cancel" ${disableIfFinal(req.status)}>Cancelar</button>
                <button class="btn ghost" data-action="complete" ${disableIfFinal(req.status)}>Encerrar</button>
                <button class="btn primary" data-action="rate" ${req.status === STATUS.Completed && !alreadyRated ? "" : "disabled"} ${alreadyRated ? "hidden" : ""}>Avaliar</button>
            </footer>
        </article>`;
    }

    function disableIfFinal(status) {
        return status === STATUS.Completed || status === STATUS.Cancelled || status === STATUS.Rejected
            ? "disabled"
            : "";
    }

    function bindActions(list) {
        list.querySelectorAll("[data-action]").forEach(btn => {
            btn.addEventListener("click", async event => {
                const action = event.currentTarget.dataset.action;
                const card = event.currentTarget.closest("[data-id]");
                const id = Number(card?.dataset.id);
                const professionalId = Number(card?.dataset.professionalId);
                if (!id) return;
                if (action === "cancel") {
                    await updateStatus(id, STATUS.Cancelled);
                }
                if (action === "complete") {
                    const code = window.prompt("Informe o codigo fornecido pelo profissional para concluir:", "");
                    if (!code || !code.trim()) {
                        ApiClient.notify("O codigo de confirmacao e obrigatorio para encerrar.", "error");
                        return;
                    }
                    await updateStatus(id, STATUS.Completed, code.trim());
                }
                if (action === "rate") {
                    openRatingModal(id, professionalId);
                    return;
                }
                await loadRequests();
            });
        });
    }

    async function updateStatus(id, status, verificationCode) {
        try {
            const payload = { id, status };
            if (verificationCode) {
                payload.verificationCode = verificationCode;
            }
            await ApiClient.put(`/service-requests/${id}/status`, payload);
            ApiClient.notify("Status atualizado", "success");
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao atualizar status", "error");
        }
    }

    function openRatingModal(serviceRequestId, professionalId) {
        const template = document.getElementById("rating-template");
        if (!template) return;
        const modal = template.content.firstElementChild.cloneNode(true);
        const overlay = document.createElement("div");
        overlay.className = "modal-overlay";
        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        const ratings = {
            quality: 5,
            speed: 5,
            communication: 5,
            professionalism: 5
        };

        modal.querySelectorAll(".rating-group").forEach(group => {
            const criteria = group.dataset.criteria;
            group.querySelectorAll(".stars button").forEach(btn => {
                btn.addEventListener("click", () => {
                    const value = Number(btn.dataset.value);
                    ratings[criteria] = value;
                    group.querySelectorAll(".stars button").forEach(b => {
                        b.classList.toggle("active", Number(b.dataset.value) <= value);
                    });
                });
            });
        });

        modal.querySelector('[data-role="cancel"]').addEventListener("click", () => overlay.remove());
        modal.querySelector('[data-role="submit"]').addEventListener("click", async () => {
            const comment = modal.querySelector('[data-role="comment"]').value;
            await submitReview(serviceRequestId, professionalId, ratings, comment);
            overlay.remove();
            await loadRequests();
        });
    }

    async function submitReview(serviceRequestId, professionalId, ratings, comment) {
        if (!state.client) return;
        const { quality, speed, communication, professionalism } = ratings;
        const priceRating = Math.round((quality + speed + communication + professionalism) / 4);
        const payload = {
            clientId: state.client.id,
            professionalId,
            serviceRequestId,
            priceRating,
            qualityRating: quality,
            speedRating: speed,
            communicationRating: communication,
            professionalismRating: professionalism,
            comment
        };
        try {
            await ApiClient.post("/reviews", payload);
            ApiClient.notify("Avaliacao enviada", "success");
            markAsRated(serviceRequestId);
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao enviar avaliacao", "error");
        }
    }

    function ratedKey() {
        return "maocerta.rated.requests";
    }

    function readRated() {
        try {
            const value = localStorage.getItem(ratedKey());
            return value ? new Set(JSON.parse(value)) : new Set();
        } catch {
            return new Set();
        }
    }

    function persistRated(set) {
        localStorage.setItem(ratedKey(), JSON.stringify(Array.from(set)));
    }

    function hasRated(id) {
        return readRated().has(id);
    }

    function markAsRated(id) {
        const set = readRated();
        set.add(id);
        persistRated(set);
    }
})(window, document, window.ApiClient, window.Auth);
