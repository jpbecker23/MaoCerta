using MaoCerta.Web.Configuration;
using MaoCerta.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

[Route("Professionals")]
public class ProfessionalsController : BaseController
{
    public ProfessionalsController(IOptions<ApiSettings> options) : base(options)
    {
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Profissionais";
        return View();
    }

    [HttpGet("{id:int}")]
    public IActionResult Details(int id)
    {
        var viewModel = new ProfessionalDetailViewModel { ProfessionalId = id };
        ViewData["Title"] = "Perfil do Profissional";
        return View(viewModel);
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        ViewData["Title"] = "Cadastro profissional";
        return View();
    }
}


