using MaoCerta.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace MaoCerta.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private readonly ApiSettings _apiSettings;

        protected BaseController(IOptions<ApiSettings> options)
        {
            _apiSettings = options.Value;
        }

        protected string ApiBaseUrl => _apiSettings.BaseUrl.TrimEnd('/');

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["ApiBaseUrl"] = ApiBaseUrl;
            base.OnActionExecuting(context);
        }
    }
}

