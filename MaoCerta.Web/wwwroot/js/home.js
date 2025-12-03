(function (window, document, ApiClient) {
    function $(id) {
        return document.getElementById(id);
    }

    document.addEventListener("DOMContentLoaded", () => {
        const container = $("home-page");
        if (!container) {
            return;
        }

        bindEvents();
        loadHomeData();
    });

    function bindEvents() {
        const searchForm = $("home-search-form");
        if (searchForm) {
            searchForm.addEventListener("submit", handleSearchSubmit);
        }
        $("refresh-categories")?.addEventListener("click", loadCategories);
        $("refresh-reviews")?.addEventListener("click", loadRecentReviews);
    }

    function handleSearchSubmit(event) {
        event.preventDefault();
        const term = $("home-search-term")?.value ?? "";
        const category = $("home-category-filter")?.value ?? "";
        const query = ApiClient.buildQuery({ searchTerm: term, categoryId: category });
        window.location.href = `/Professionals${query}`;
    }

    function loadHomeData() {
        loadCategories();
        loadFeaturedProfessionals();
        loadRecentReviews();
        loadServiceFeed();
        loadHeroStats();
    }

    async function loadCategories() {
        const grid = $("categories-grid");
        if (!grid) return;
        grid.innerHTML = "";
        try {
            const categories = await ApiClient.get("/categories");
            if (!categories?.length) {
                renderEmpty(grid);
                return;
            }
            grid.innerHTML = categories.slice(0, 8).map(categoryCard).join("\n");
            const select = $("home-category-filter");
            if (select) {
                const current = select.value;
                select.innerHTML = '<option value="">Todas as categorias</option>' + categories
                    .map(category => `<option value="${category.id}">${category.name}</option>`)
                    .join("\n");
                if (current) {
                    select.value = current;
                }
            }
        } catch (error) {
            renderError(grid, error.message);
        }
    }

    function categoryCard(category) {
        return `<article class="card category">
            <h3>${category.name}</h3>
            <p>${category.description ?? ""}</p>
            <small>${category.professionalCount ?? 0} profissionais</small>
        </article>`;
    }

    async function loadFeaturedProfessionals() {
        const grid = $("featured-professionals");
        if (!grid) return;
        grid.innerHTML = "";
        try {
            const professionals = await ApiClient.get("/professionals/top-rated?count=6");
            if (!professionals?.length) {
                renderEmpty(grid);
                return;
            }
            grid.innerHTML = professionals.map(pro => `
                <article class="professional-card">
                    <header>
                        <div>
                            <h3>${pro.name}</h3>
                            <small>${pro.categoryName}</small>
                        </div>
                        <div class="score">${(pro.averageRating ?? 0).toFixed(1)}</div>
                    </header>
                    <p class="muted">${pro.description ?? "Ainda sem descrição."}</p>
                    <footer>
                        <span>${pro.totalReviews ?? 0} avaliações</span>
                        <a class="btn ghost" href="/Professionals/${pro.id}">Ver perfil</a>
                    </footer>
                </article>`).join("\n");
        } catch (error) {
            renderError(grid, error.message);
        }
    }

    async function loadRecentReviews() {
        const container = $("recent-reviews");
        if (!container) return;
        container.innerHTML = "";
        try {
            const reviews = await ApiClient.get("/reviews");
            if (!reviews?.length) {
                renderEmpty(container);
                return;
            }
            const items = reviews.slice(0, 6).map(review => `
                <article>
                    <div class="review-meta">
                        <strong>${review.clientName}</strong>
                        <span>${new Date(review.createdAt).toLocaleDateString()}</span>
                    </div>
                    <p>${review.comment ?? "Sem comentário"}</p>
                    <small>${review.professionalName} · Nota ${(review.overallRating ?? 0).toFixed(1)}</small>
                </article>`);
            container.innerHTML = items.join("\n");
        } catch (error) {
            renderError(container, error.message);
        }
    }

    async function loadServiceFeed() {
        const grid = $("open-requests");
        if (!grid) return;
        grid.innerHTML = "";
        try {
            const requests = await ApiClient.get("/service-requests", { suppressUnauthorized: true });
            if (!requests?.length) {
                renderEmpty(grid);
                return;
            }
            grid.innerHTML = requests.slice(0, 4).map(request => `
                <article class="card">
                    <h3>${request.title}</h3>
                    <p class="muted">${request.description ?? ""}</p>
                    <small>Status: ${request.status}</small>
                </article>`).join("\n");
        } catch (error) {
            renderError(grid, error.message);
        }
    }

    async function loadHeroStats() {
        try {
            const professionals = await ApiClient.get("/professionals");
            const reviews = await ApiClient.get("/reviews");
            const requests = await ApiClient.get("/service-requests", { suppressUnauthorized: true });
            $("total-professionals").textContent = professionals?.length ?? 0;
            $("total-requests").textContent = requests?.length ?? 0;
            const average = reviews?.length
                ? reviews.reduce((sum, item) => sum + (item.overallRating ?? 0), 0) / reviews.length
                : 0;
            $("avg-rating").textContent = average.toFixed(1);
        } catch {
            // Falha silenciosa, os valores padrão permanecem
        }
    }

    function renderEmpty(container) {
        const message = container.dataset.emptyMessage ?? "Sem registros";
        container.innerHTML = `<p class="muted">${message}</p>`;
    }

    function renderError(container, message) {
        container.innerHTML = `<p class="muted">${message}</p>`;
    }
})(window, document, window.ApiClient);

