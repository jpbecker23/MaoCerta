using MaoCerta.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(IOptions<ApiSettings> options, ILogger<HomeController> logger) : base(options)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("Rendering landing page for Mao Certa Web");
        return View();
    }
}

