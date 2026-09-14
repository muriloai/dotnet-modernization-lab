using System.Diagnostics;
using System.Web.Mvc;
using LoggingDemo.Models;
using LoggingDemo.Services;

namespace LoggingDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProcessadorPedidosLegado _processador = new ProcessadorPedidosLegado();

        [HttpGet]
        public ActionResult Index()
        {
            var modelo = new PedidoLegado();
            ViewBag.HistoricoLogs = MemoriaTraceListener.ObterHistorico();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Processar(PedidoLegado pedido, bool simularAlerta = false, bool simularErro = false)
        {
            Trace.TraceInformation("Requisição recebida na Action HomeController.Processar para o pedido " + pedido.PedidoId);

            _processador.Processar(pedido, simularAlerta, simularErro);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Limpar()
        {
            MemoriaTraceListener.LimparHistorico();
            return RedirectToAction("Index");
        }
    }
}
