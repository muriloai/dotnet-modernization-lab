using System.Web.Mvc;

namespace RestApisDemo.Controllers
{
    /// <summary>
    /// Controller MVC tradicional responsável por renderizar o painel interativo de testes da Web API 2.
    /// </summary>
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
