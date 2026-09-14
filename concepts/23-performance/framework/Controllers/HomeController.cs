using System.Web.Mvc;
using PerformanceDemo.Services;

namespace PerformanceDemo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ParserLogLegadoService _service = new ParserLogLegadoService();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExecutarBenchmarkParsing(int linhas = 50000)
        {
            if (linhas < 5000 || linhas > 200000)
            {
                linhas = 50000;
            }

            var resultado = _service.ExecutarParsingComSubstring(linhas);
            ViewBag.ResultadoParsing = resultado;
            ViewBag.LinhasInformadas = linhas;

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExecutarBenchmarkBuffers(int iteracoes = 15000, int tamanhoKb = 64)
        {
            if (iteracoes < 1000 || iteracoes > 50000)
            {
                iteracoes = 15000;
            }

            if (tamanhoKb < 4 || tamanhoKb > 256)
            {
                tamanhoKb = 64;
            }

            var resultado = _service.ExecutarAlocacaoBuffers(iteracoes, tamanhoKb);
            ViewBag.ResultadoBuffers = resultado;
            ViewBag.IteracoesInformadas = iteracoes;
            ViewBag.TamanhoKbInformado = tamanhoKb;

            return View("Index");
        }
    }
}
