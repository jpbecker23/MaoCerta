using MaoCerta.Web.Configuration;
using MaoCerta.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

[Route("ServiceRequests")]
public class ServiceRequestsController : BaseController
{
    public ServiceRequestsController(IOptions<ApiSettings> options) : base(options)
    {
    }

    [HttpGet("")]
    public IActionResult Index(int? professionalId = null, string? professionalName = null)
    {
        return RedirectToAction(nameof(Create), new { professionalId, professionalName });
    }

    [HttpGet("Create")]
    public IActionResult Create(int? professionalId = null, string? professionalName = null)
    {
        var viewModel = new ServiceRequestViewModel
        {
            ProfessionalId = professionalId,
            ProfessionalName = professionalName
        };

        ViewData["Title"] = "Solicitar Serviço";
        return View(viewModel);
    }

    [HttpGet("My")]
    public IActionResult My()
    {
        ViewData["Title"] = "Minhas solicitações";
        return View();
    }

    [HttpGet("Inbox")]
    public IActionResult Inbox()
    {
        ViewData["Title"] = "Solicitações recebidas";
        return View();
    }
}
