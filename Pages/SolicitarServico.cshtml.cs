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
    public class SolicitarServicoModel : PageModel
    {
        private readonly AppDbContext _context;

        public SolicitarServicoModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SolicitacaoInputModel Input { get; set; } = new();

        public Models.Profissional? Profissional { get; set; }
        public string? Mensagem { get; set; }
        public bool Sucesso { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Profissional = await _context.Profissionais
                .Include(p => p.Categoria)
                .Include(p => p.Avaliacoes)
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
                var solicitacao = new SolicitacaoServico
                {
                    ClienteId = Input.ClienteId,
                    ProfissionalId = Input.ProfissionalId,
                    Titulo = Input.Titulo,
                    Descricao = Input.Descricao,
                    EnderecoServico = Input.EnderecoServico,
                    DataAgendamento = Input.DataAgendamento,
                    ValorProposto = Input.ValorProposto,
                    Observacoes = Input.Observacoes,
                    Status = "Pendente",
                    CodigoVerificacao = GerarCodigoVerificacao()
                };

                _context.SolicitacoesServico.Add(solicitacao);
                await _context.SaveChangesAsync();

                Sucesso = true;
                return RedirectToPage("/Profissionais");
            }
            catch (Exception ex)
            {
                await CarregarProfissional();
                Mensagem = $"Erro ao salvar solicitação: {ex.Message}";
            }

            return Page();
        }

        private async Task CarregarProfissional()
        {
            Profissional = await _context.Profissionais
                .Include(p => p.Categoria)
                .Include(p => p.Avaliacoes)
                .FirstOrDefaultAsync(p => p.Id == Input.ProfissionalId);
        }

        private string GerarCodigoVerificacao()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public class SolicitacaoInputModel
        {
            [Required]
            public int ClienteId { get; set; }

            [Required]
            public int ProfissionalId { get; set; }

            [Required, MaxLength(200)]
            public string Titulo { get; set; } = string.Empty;

            [MaxLength(1000)]
            public string? Descricao { get; set; }

            [MaxLength(200)]
            public string? EnderecoServico { get; set; }

            public DateTime? DataAgendamento { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? ValorProposto { get; set; }

            [MaxLength(1000)]
            public string? Observacoes { get; set; }
        }
    }
}
