using Microsoft.AspNetCore.Mvc.RazorPages;
using MaoCerta.Data;
using Microsoft.EntityFrameworkCore;

namespace MaoCerta.Pages
{
    public class ProfissionaisModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProfissionaisModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Models.Profissional> Profissionais { get; set; } = new();
        public List<Models.Categoria> Categorias { get; set; } = new();

        public async Task OnGetAsync()
        {
            Profissionais = await _context.Profissionais
                .Include(p => p.Categoria)
                .Include(p => p.Avaliacoes)
                .Where(p => p.Ativo)
                .ToListAsync();

            Categorias = await _context.Categorias
                .Where(c => c.Ativa)
                .ToListAsync();
        }
    }
}
