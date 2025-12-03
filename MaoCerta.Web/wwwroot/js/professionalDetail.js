(function (window, document, ApiClient, Auth) {
    const state = {
        professional: null,
        reviews: []
    };

    document.addEventListener("DOMContentLoaded", () => {
        const section = document.getElementById("professional-detail");
        if (!section) return;
        bindFilters();
        const id = Number(section.dataset.professionalId);
        if (id) {
            loadDetail(id);
        }
    });

    function bindFilters() {
        document.getElementById("review-filter-rating")?.addEventListener("change", renderReviews);
        document.getElementById("review-filter-criteria")?.addEventListener("change", renderReviews);
    }

    async function loadDetail(id) {
        const section = document.getElementById("professional-detail");
        if (!section) return;
        section.classList.add("loading");
        try {
            const [professional, reviews] = await Promise.all([
                ApiClient.get(`/professionals/${id}`),
                ApiClient.get(`/reviews/professional/${id}`)
            ]);
            state.professional = professional;
            state.reviews = Array.isArray(reviews) ? reviews : [];
            populateHeader(professional);
            populateMetrics();
            populateCriteria();
            renderReviews();
        } catch (error) {
            ApiClient.notify(error.message || "Erro ao carregar profissional", "error");
        } finally {
            section.classList.remove("loading");
        }
    }

    function populateHeader(detail) {
        const nameEl = document.getElementById("professional-name");
        const descEl = document.getElementById("professional-description");
        const catEl = document.getElementById("professional-category");
        const statusEl = document.getElementById("professional-status");

        if (nameEl) nameEl.textContent = detail.name;
        if (descEl) descEl.textContent = cleanDescription(detail.description) ?? "Sem descrição";
        if (catEl) catEl.textContent = detail.categoryName ?? "Categoria";
        if (statusEl) statusEl.textContent = detail.isActive ? "Ativo" : "Indisponível";

        const requestLink = document.getElementById("request-service");
        if (requestLink) {
            if (isOwnProfile(detail)) {
                requestLink.hidden = true;
            } else {
                requestLink.href = `${requestLink.dataset.baseUrl}?professionalId=${detail.id}&professionalName=${encodeURIComponent(detail.name)}`;
                requestLink.hidden = false;
            }
        }
    }

    function populateMetrics() {
        const totalReviews = state.reviews.length;
        const avg = totalReviews
            ? state.reviews.reduce((sum, review) => sum + (review.overallRating ?? 0), 0) / totalReviews
            : 0;
        document.getElementById("professional-rating").textContent = avg.toFixed(1);
        document.getElementById("professional-total-reviews").textContent = totalReviews;
        document.getElementById("professional-services-done").textContent = 0;
        document.getElementById("professional-services-pending").textContent = 0;
    }

    function populateCriteria() {
        const matrix = document.getElementById("criteria-matrix");
        if (!matrix) return;
        const totals = state.reviews.reduce((acc, review) => {
            acc.quality += review.qualityRating ?? 0;
            acc.time += review.speedRating ?? 0;
            acc.communication += review.communicationRating ?? 0;
            acc.professionalism += review.professionalismRating ?? 0;
            return acc;
        }, { quality: 0, time: 0, communication: 0, professionalism: 0 });
        const divisor = state.reviews.length || 1;
        const averages = {
            quality: totals.quality / divisor,
            time: totals.time / divisor,
            communication: totals.communication / divisor,
            professionalism: totals.professionalism / divisor
        };
        Object.entries(averages).forEach(([key, value]) => {
            const slot = matrix.querySelector(`[data-criteria="${key}"] strong`);
            if (slot) {
                slot.textContent = value.toFixed(1);
            }
        });
    }

    function cleanDescription(text) {
        if (!text) return text;
        const marker = text.toLowerCase().indexOf("documentos/links:");
        if (marker >= 0) {
            return text.slice(0, marker).trim();
        }
        return text;
    }

    function renderReviews() {
        const container = document.getElementById("professional-reviews");
        if (!container) return;
        if (!state.reviews.length) {
            container.innerHTML = `<p class="muted">${container.dataset.emptyMessage}</p>`;
            return;
        }
        const minRating = parseFloat(document.getElementById("review-filter-rating")?.value || "0");
        const criteria = document.getElementById("review-filter-criteria")?.value;
        const reviews = state.reviews.filter(review => {
            if ((review.overallRating ?? 0) < minRating) return false;
            if (!criteria) return true;
            const map = {
                quality: review.qualityRating,
                time: review.speedRating,
                communication: review.communicationRating,
                professionalism: review.professionalismRating
            };
            return (map[criteria] ?? 0) >= 4;
        });
        if (!reviews.length) {
            container.innerHTML = `<p class="muted">Nenhuma avaliação neste filtro.</p>`;
            return;
        }
        container.innerHTML = reviews.slice(0, 10).map(renderReviewCard).join("\n");
    }

    function renderReviewCard(review) {
        if (!review) return "";
        const dateStr = new Date(review.createdAt ?? Date.now()).toLocaleDateString("pt-BR");
        const positives = review.positivePoints?.trim();
        const negatives = review.negativePoints?.trim();
        const points = [
            positives ? `<div><strong>+ </strong>${positives}</div>` : "",
            negatives ? `<div><strong>- </strong>${negatives}</div>` : ""
        ].filter(Boolean).join("");
        return `<article>
            <div class="review-meta">
                <strong>${review.clientName ?? "Cliente"}</strong>
                <span>${dateStr}</span>
                <span>Nota ${(review.overallRating ?? 0).toFixed(1)}</span>
            </div>
            <p>${review.comment ?? "Sem comentário"}</p>
            ${points ? `<div class="points">${points}</div>` : ""}
        </article>`;
    }
    function isOwnProfile(detail) {
        const user = Auth?.getUser?.() || ApiClient.getStoredUser?.();
        if (!user?.email || !detail?.email) return false;
        return user.email.toLowerCase() === detail.email.toLowerCase();
    }
})(window, document, window.ApiClient, window.Auth);
