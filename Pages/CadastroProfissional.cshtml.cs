using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using MaoCerta.Data;
using MaoCerta.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Pages
{
    public class CadastroProfissionalModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public CadastroProfissionalModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public RegisterProfissionalInputModel Input { get; set; } = new();

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Categorias { get; set; } = new();

        public string? Mensagem { get; set; }
        public bool Sucesso { get; set; }

        public async Task OnGetAsync()
        {
            await CarregarCategorias();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarCategorias();
                Mensagem = "Preencha todos os campos corretamente.";
                return Page();
            }

            try
            {
                // Criar usuário no Identity
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Criar profissional no banco de dados
                    var profissional = new Profissional
                    {
                        Nome = Input.Nome,
                        Email = Input.Email,
                        Telefone = Input.Telefone,
                        Endereco = Input.Endereco,
                        Descricao = Input.Descricao,
                        CategoriaId = Input.CategoriaId
                    };

                    _context.Profissionais.Add(profissional);
                    await _context.SaveChangesAsync();

                    // Fazer login automático
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    Sucesso = true;
                    return RedirectToPage("/Home");
                }
                else
                {
                    await CarregarCategorias();
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    Mensagem = "Erro ao criar conta. Verifique os dados e tente novamente.";
                }
            }
            catch (Exception ex)
            {
                await CarregarCategorias();
                Mensagem = $"Erro ao salvar: {ex.Message}";
            }

            return Page();
        }

        private async Task CarregarCategorias()
        {
            var categorias = await _context.Categorias
                .Where(c => c.Ativa)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();

            Categorias = categorias;
        }

        public class RegisterProfissionalInputModel
        {
            [Required, MaxLength(100)]
            public string Nome { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MaxLength(15)]
            public string Telefone { get; set; } = string.Empty;

            [Required, MaxLength(200)]
            public string Endereco { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Descricao { get; set; }

            [Required]
            public int CategoriaId { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
