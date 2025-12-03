using MaoCerta.Web.Configuration;
using MaoCerta.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

public class ReviewsController : BaseController
{
    public ReviewsController(IOptions<ApiSettings> options) : base(options)
    {
    }

    [HttpGet]
    public IActionResult Create(int? professionalId = null, int? serviceRequestId = null)
    {
        var viewModel = new ReviewFormViewModel
        {
            ProfessionalId = professionalId,
            ServiceRequestId = serviceRequestId
        };

        ViewData["Title"] = "Avaliar profissional";
        return View(viewModel);
    }
}

