using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using MaoCerta.Data;
using MaoCerta.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Pages
{
    [Authorize]
    public class AvaliarProfissionalModel : PageModel
    {
        private readonly AppDbContext _context;

        public AvaliarProfissionalModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AvaliacaoInputModel Input { get; set; } = new();

        public Models.Profissional? Profissional { get; set; }
        public string? Mensagem { get; set; }
        public bool Sucesso { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Profissional = await _context.Profissionais
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Profissional == null)
            {
                return NotFound();
            }

            Input.ProfissionalId = id;
            // Aqui você pode obter o ClienteId do usuário logado
            // Por enquanto, vou usar um valor fixo para demonstração
            Input.ClienteId = 1; // Em um sistema real, obteria do usuário logado

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarProfissional();
                Mensagem = "Preencha todos os campos obrigatórios.";
                return Page();
            }

            try
            {
                var avaliacao = new Avaliacao
                {
                    ClienteId = Input.ClienteId,
                    ProfissionalId = Input.ProfissionalId,
                    SolicitacaoServicoId = Input.SolicitacaoServicoId,
                    NotaPreco = Input.NotaPreco,
                    NotaQualidade = Input.NotaQualidade,
                    NotaTempoExecucao = Input.NotaTempoExecucao,
                    NotaComunicacao = Input.NotaComunicacao,
                    NotaProfissionalismo = Input.NotaProfissionalismo,
                    Comentario = Input.Comentario,
                    PontosPositivos = Input.PontosPositivos,
                    PontosNegativos = Input.PontosNegativos
                };

                _context.Avaliacoes.Add(avaliacao);
                await _context.SaveChangesAsync();

                Sucesso = true;
                return RedirectToPage("/Profissionais");
            }
            catch (Exception ex)
            {
                await CarregarProfissional();
                Mensagem = $"Erro ao salvar avaliação: {ex.Message}";
            }

            return Page();
        }

        private async Task CarregarProfissional()
        {
            Profissional = await _context.Profissionais
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == Input.ProfissionalId);
        }

        public class AvaliacaoInputModel
        {
            [Required]
            public int ClienteId { get; set; }

            [Required]
            public int ProfissionalId { get; set; }

            [Required]
            public int SolicitacaoServicoId { get; set; }

            [Required, Range(1, 5)]
            public int NotaPreco { get; set; }

            [Required, Range(1, 5)]
            public int NotaQualidade { get; set; }

            [Required, Range(1, 5)]
            public int NotaTempoExecucao { get; set; }

            [Required, Range(1, 5)]
            public int NotaComunicacao { get; set; }

            [Required, Range(1, 5)]
            public int NotaProfissionalismo { get; set; }

            [MaxLength(1000)]
            public string? Comentario { get; set; }

            [MaxLength(500)]
            public string? PontosPositivos { get; set; }

            [MaxLength(500)]
            public string? PontosNegativos { get; set; }
        }
    }
}
