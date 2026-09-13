using System.Web.Mvc;

namespace MiddlewareVsModulesDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Servidor = Request.ServerVariables["SERVER_SOFTWARE"] ?? "IIS Express";
            ViewBag.Metodo = Request.HttpMethod;
            ViewBag.Url = Request.Url?.ToString();

            return View();
        }
    }
}
