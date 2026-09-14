using System.Threading.Tasks;
using System.Web.Mvc;
using AsyncAwaitDemo.Services;

namespace AsyncAwaitDemo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly OperacoesAssincronasLegadoService _service = new OperacoesAssincronasLegadoService();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ObterLote(int total = 5)
        {
            if (total < 1 || total > 20)
            {
                total = 5;
            }

            // O cliente aguarda todo o lote ser montado na memória antes de receber o HTML
            var eventos = await _service.ObterLoteCompletoAsync(total, 250);
            ViewBag.Eventos = eventos;
            ViewBag.TotalSolicitado = total;
            ViewBag.AvisoBuffering = "Todo o lote de " + total + " registros foi carregado na memória do servidor antes do envio da resposta.";

            return View("Index");
        }
    }
}
