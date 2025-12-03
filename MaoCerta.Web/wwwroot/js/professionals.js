(function (window, document, ApiClient, Auth) {
    const state = {
        page: 1,
        pageSize: 6,
        total: 0,
        professionals: [],
        detailCache: new Map(),
        isProfessional: false
    };

    document.addEventListener("DOMContentLoaded", () => {
        const container = document.getElementById("professionals-page");
        if (!container) return;
        resolveUserProfile().then(() => {
            applyQueryToFilters();
            bindFilterHandlers();
            loadCategories();
            fetchProfessionals();
        });
    });

    async function resolveUserProfile() {
        const user = Auth?.getUser?.() || ApiClient.getStoredUser?.();
        if (!Auth || !user?.email) {
            state.isProfessional = false;
            return;
        }
        try {
            const professionals = await ApiClient.get("/professionals");
            state.isProfessional = professionals?.some?.(
                p => (p.email || "").toLowerCase() === user.email.toLowerCase()
            ) || false;
        } catch {
            state.isProfessional = false;
        }
    }

    function applyQueryToFilters() {
        const params = new URLSearchParams(window.location.search);
        const searchTerm = params.get("searchTerm");
        const categoryId = params.get("categoryId");
        if (searchTerm) {
            const input = document.querySelector('[name="searchTerm"]');
            if (input) input.value = searchTerm;
        }
        if (categoryId) {
            const select = document.querySelector('[name="categoryId"]');
            if (select) select.value = categoryId;
        }
    }

    function bindFilterHandlers() {
        const form = document.getElementById("professional-filter-form");
        const range = document.getElementById("filters-rating");
        const rangeValue = document.getElementById("filters-rating-value");
        if (range && rangeValue) {
            const updateValue = () => {
                rangeValue.textContent = range.value;
            };
            range.addEventListener("input", updateValue);
            updateValue();
        }
        form?.addEventListener("submit", event => {
            event.preventDefault();
            state.page = 1;
            fetchProfessionals();
        });
        document.getElementById("reset-filters")?.addEventListener("click", () => {
            form?.reset();
            if (rangeValue) {
                rangeValue.textContent = "0";
            }
            state.page = 1;
            fetchProfessionals();
        });
        const pagination = document.getElementById("professional-pagination");
        pagination?.addEventListener("click", event => {
            const button = event.target.closest("button[data-action]");
            if (!button) return;
            const action = button.dataset.action;
            if (action === "prev" && state.page > 1) {
                state.page--;
                fetchProfessionals();
            }
            if (action === "next" && state.page < Math.ceil(state.total / state.pageSize)) {
                state.page++;
                fetchProfessionals();
            }
        });
    }

    async function loadCategories() {
        const select = document.getElementById("filters-category");
        if (!select) return;
        try {
            const categories = await ApiClient.get("/categories");
            select.innerHTML = '<option value="">Todas</option>' + (categories || [])
                .map(c => `<option value="${c.id}">${c.name}</option>`)
                .join("\n");
        } catch {
            select.innerHTML = '<option value="">Todas</option>';
        }
    }

    function getFilters() {
        const form = document.getElementById("professional-filter-form");
        if (!form) return {};
        const data = new FormData(form);
        const criteria = Array.from(form.querySelectorAll('input[name="criteria"]:checked')).map(input => input.value);
        return {
            searchTerm: data.get("searchTerm")?.toString() ?? "",
            categoryId: data.get("categoryId")?.toString() ?? "",
            minRating: parseFloat(data.get("minRating")) || 0,
            sortBy: data.get("sortBy")?.toString() ?? "rating",
            sortDirection: data.get("sortDirection")?.toString() ?? "desc",
            criteria
        };
    }

    async function fetchProfessionals() {
        const list = document.getElementById("professional-results");
        if (!list) return;
        list.innerHTML = '<p class="muted">Carregando...</p>';
        const filters = getFilters();
        try {
            const payload = {
                searchTerm: filters.searchTerm,
                categoryId: filters.categoryId ? Number(filters.categoryId) : null,
                minRating: filters.minRating > 0 ? filters.minRating : null,
                sortBy: filters.sortBy,
                sortDirection: filters.sortDirection,
                page: state.page,
                pageSize: state.pageSize
            };
            const professionals = await ApiClient.post("/professionals/search", payload);
            state.professionals = professionals ?? [];
            state.total = state.professionals.length;
            await enrichWithDetails(filters.criteria);
            state.total = state.professionals.length;
            renderProfessionals(filters.criteria);
            renderPagination();
        } catch (error) {
            list.innerHTML = `<p class="muted">${error.message}</p>`;
        }
    }

    async function enrichWithDetails(criteriaFilters) {
        const ids = state.professionals.map(p => p.id);
        await Promise.all(ids.map(async id => {
            if (state.detailCache.has(id)) {
                return;
            }
            try {
                const reviews = await ApiClient.get(`/reviews/professional/${id}`);
                const summary = summarizeReviews(reviews ?? []);
                state.detailCache.set(id, summary);
            } catch {
                state.detailCache.set(id, summarizeReviews([]));
            }
        }));
        if (criteriaFilters?.length) {
            state.professionals = state.professionals.filter(p => {
                const detail = state.detailCache.get(p.id);
                if (!detail) return false;
                return criteriaFilters.every(criteria => (detail[criteria] ?? 0) >= 4);
            });
        }
    }

    function summarizeReviews(reviews) {
        if (!reviews.length) {
            return { quality: 0, time: 0, communication: 0, professionalism: 0 };
        }
        const totals = reviews.reduce((acc, review) => {
            acc.quality += review.qualityRating ?? 0;
            acc.time += review.speedRating ?? 0;
            acc.communication += review.communicationRating ?? 0;
            acc.professionalism += review.professionalismRating ?? 0;
            return acc;
        }, { quality: 0, time: 0, communication: 0, professionalism: 0 });
        return {
            quality: totals.quality / reviews.length,
            time: totals.time / reviews.length,
            communication: totals.communication / reviews.length,
            professionalism: totals.professionalism / reviews.length
        };
    }

    function renderProfessionals(criteriaFilters) {
        const list = document.getElementById("professional-results");
        if (!list) return;
        if (!state.professionals.length) {
            list.innerHTML = `<p class="muted">${list.dataset.emptyMessage}</p>`;
            return;
        }
        const cards = state.professionals.map(pro => {
            const detail = state.detailCache.get(pro.id);
            const criteria = detail ? criteriaBadges(detail) : "";
            const requestCta = state.isProfessional
                ? '<button class="btn primary" disabled title="Disponivel apenas para clientes">Solicitar</button>'
                : `<a class="btn primary" href="/ServiceRequests?professionalId=${pro.id}&professionalName=${encodeURIComponent(pro.name)}">Solicitar</a>`;
            return `<article class="professional-card">
                <header>
                    <div>
                        <h3>${pro.name}</h3>
                        <small>${pro.categoryName}</small>
                    </div>
                    <div class="score">${(pro.averageRating ?? 0).toFixed(1)}</div>
                </header>
                <p class="muted">${cleanDescription(pro.description) ?? "Descricao indisponivel"}</p>
                <div class="criteria-breakdown">${criteria}</div>
                <footer>
                    <span>${pro.totalReviews ?? 0} avaliacoes</span>
                    <div>
                        <a class="btn ghost" href="/Professionals/${pro.id}">Ver perfil</a>
                        ${requestCta}
                    </div>
                </footer>
            </article>`;
        });
        list.innerHTML = cards.join("\n");
        document.getElementById("result-count").textContent = `${state.professionals.length} profissionais`;
        const avg = state.professionals.reduce((sum, pro) => sum + (pro.averageRating ?? 0), 0) / state.professionals.length;
        document.getElementById("result-average").textContent = `media ${avg.toFixed(1)}`;
    }

    function criteriaBadges(detail) {
        if (!detail) {
            return "";
        }
        const entries = [
            ["Qualidade", detail.quality],
            ["Pontualidade", detail.time],
            ["Comunicacao", detail.communication],
            ["Profissionalismo", detail.professionalism]
        ];
        return entries.map(([label, value]) => `<span>${label} ${(value ?? 0).toFixed(1)}</span>`).join("\n");
    }

    function cleanDescription(text) {
        if (!text) return text;
        const marker = text.toLowerCase().indexOf("documentos/links:");
        if (marker >= 0) {
            return text.slice(0, marker).trim();
        }
        return text;
    }
    function renderPagination() {
        const pagination = document.getElementById("professional-pagination");
        if (!pagination) return;
        const totalPages = Math.max(1, Math.ceil(state.total / state.pageSize));
        pagination.hidden = totalPages <= 1;
        document.getElementById("pagination-info").textContent = `Pagina ${state.page} de ${totalPages}`;
    }
})(window, document, window.ApiClient, window.Auth);
