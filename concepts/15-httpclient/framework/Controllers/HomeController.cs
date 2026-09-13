using System.Web.Mvc;

namespace HttpClientDemo.Controllers
{
    /// <summary>
    /// Controller MVC tradicional para renderizar o painel interativo de testes do laboratório legado.
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
