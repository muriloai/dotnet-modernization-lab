using System.Web.Mvc;

namespace SignalRDemo.Controllers
{
    /// <summary>
    /// Controller MVC tradicional para renderizar o painel interativo de testes do SignalR 2.x.
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
