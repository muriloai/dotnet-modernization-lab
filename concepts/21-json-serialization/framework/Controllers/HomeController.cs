using System;
using System.Collections.Generic;
using System.Web.Mvc;
using JsonSerializationDemo.Models;
using JsonSerializationDemo.Services;
using Newtonsoft.Json;

namespace JsonSerializationDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly SerializadorLegadoService _service = new SerializadorLegadoService();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.QuantidadePadrao = 5000;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExecutarBenchmark(int quantidade = 5000)
        {
            if (quantidade <= 0 || quantidade > 50000)
            {
                quantidade = 5000;
            }

            var lote = _service.GerarLote(quantidade);

            double tempoJs;
            string jsonJs = _service.SerializarComJavaScriptSerializer(lote, out tempoJs);

            double tempoNewtonsoft;
            string jsonNewtonsoft = _service.SerializarComNewtonsoft(lote, out tempoNewtonsoft);

            ViewBag.Quantidade = quantidade;
            ViewBag.TempoJs = tempoJs;
            ViewBag.TamanhoJsKb = Math.Round((double)jsonJs.Length / 1024.0, 2);
            ViewBag.TempoNewtonsoft = tempoNewtonsoft;
            ViewBag.TamanhoNewtonsoftKb = Math.Round((double)jsonNewtonsoft.Length / 1024.0, 2);
            ViewBag.SucessoBenchmark = true;

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestarPolimorfismo(string tipo, decimal valor, string chavePix, string numeroCartao, int parcelas)
        {
            var pagamento = new PagamentoLegado
            {
                Tipo = tipo ?? "pix",
                Valor = valor > 0 ? valor : 150.50m,
                DataHora = DateTime.Now,
                ChavePix = chavePix,
                NumeroCartao = numeroCartao,
                Parcelas = parcelas
            };

            double tempoMs;
            string json = _service.SerializarComNewtonsoft(pagamento, out tempoMs);

            ViewBag.ResultadoPolimorfismoJson = json;
            ViewBag.TipoInformado = tipo;
            ViewBag.AvisoSeguranca = "No .NET Framework legado, deserialização polimórfica exigia TypeNameHandling ou modelos achatados com propriedades nulas.";

            return View("Index");
        }

        [HttpGet]
        public ActionResult BaixarJson(int quantidade = 1000)
        {
            if (quantidade <= 0 || quantidade > 20000)
            {
                quantidade = 1000;
            }

            var lote = _service.GerarLote(quantidade);
            double tempoMs;
            string json = _service.SerializarComNewtonsoft(lote, out tempoMs);

            return Content(json, "application/json", System.Text.Encoding.UTF8);
        }
    }
}
