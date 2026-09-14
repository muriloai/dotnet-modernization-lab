using System.Web.Mvc;

namespace ActionFiltersDemo.Controllers
{
    /// <summary>
    /// Controller MVC responsável pela interface interativa de testes no projeto legado.
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
