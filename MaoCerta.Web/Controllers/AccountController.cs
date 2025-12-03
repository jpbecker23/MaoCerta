using MaoCerta.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

public class AccountController : BaseController
{
    public AccountController(IOptions<ApiSettings> options) : base(options)
    {
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Entrar";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "Criar conta";
        return View();
    }

    [HttpGet]
    [Route("RegisterClient")]
    [Route("register-client")]
    public IActionResult RegisterClient()
    {
        ViewData["Title"] = "Cadastro de cliente";
        return View("RegisterClient");
    }

    [HttpGet]
    [Route("RegisterProfessional")]
    [Route("register-professional")]
    public IActionResult RegisterProfessional()
    {
        ViewData["Title"] = "Cadastro de profissional";
        return View("RegisterProfessional");
    }

    [HttpGet]
    [Route("Profile")]
    public IActionResult Profile()
    {
        ViewData["Title"] = "Meu perfil";
        return View();
    }
}

