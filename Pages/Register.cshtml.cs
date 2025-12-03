using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using MaoCerta.Data;
using MaoCerta.Models;
using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public string? Mensagem { get; set; }
        public bool Sucesso { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Mensagem = "Preencha todos os campos corretamente.";
                return Page();
            }

            try
            {
                // Criar usuário no Identity
                var user = new IdentityUser { UserName = Input.Nome, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Criar cliente no banco de dados
                    var cliente = new Cliente
                    {
                        Nome = Input.Nome,
                        Email = Input.Email,
                        Telefone = Input.Telefone,
                        Endereco = Input.Endereco,
                        Idade = Input.Idade
                    };

                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();

                    // Fazer login automático
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    Sucesso = true;
                    return RedirectToPage("/Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    Mensagem = "Erro ao criar conta. Verifique os dados e tente novamente.";
                }
            }
            catch (Exception ex)
            {
                Mensagem = $"Erro ao salvar: {ex.Message}";
            }

            return Page();
        }

        public class RegisterInputModel
        {
            [Required, MaxLength(40)]
            public string Nome { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MaxLength(15)]
            public string Telefone { get; set; } = string.Empty;

            [MaxLength(200)]
            public string? Endereco { get; set; }

            public int? Idade { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
