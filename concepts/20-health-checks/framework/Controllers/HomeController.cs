using System.Web.Mvc;
using HealthChecksDemo.Handlers;

namespace HealthChecksDemo.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.FalhaBanco = PingHandler.SimularFalhaBanco;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlternarBanco(bool falha)
        {
            PingHandler.SimularFalhaBanco = falha;
            return RedirectToAction("Index");
        }
    }
}
