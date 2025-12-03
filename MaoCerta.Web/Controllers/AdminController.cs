using MaoCerta.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

public class AdminController : BaseController
{
    public AdminController(IOptions<ApiSettings> options) : base(options)
    {
    }

    [HttpGet]
    public IActionResult Reviews()
    {
        ViewData["Title"] = "Moderação de Avaliações";
        return View();
    }
}

