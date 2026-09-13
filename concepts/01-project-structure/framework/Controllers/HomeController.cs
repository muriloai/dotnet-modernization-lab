using System.Configuration;
using System.Web.Mvc;

namespace ProjectStructure.Controllers
{
    /// <summary>
    /// Controller MVC tradicional herdando de System.Web.Mvc.Controller.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Leitura estática e não-tipada através de ConfigurationManager
            ViewBag.ApplicationName = ConfigurationManager.AppSettings["ApplicationName"] ?? "ASP.NET MVC 5";
            ViewBag.Environment = ConfigurationManager.AppSettings["Environment"] ?? "Development";
            ViewBag.ServerSoftware = Request.ServerVariables["SERVER_SOFTWARE"] ?? "Microsoft-IIS (Simulado)";

            return View();
        }
    }
}
