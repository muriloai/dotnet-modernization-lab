using System;
using System.Diagnostics;
using System.Web.Mvc;
using JitOverheadDemo.Models;

namespace JitOverheadDemo.Controllers
{
    public class JitDemoLegadoController : Controller
    {
        public ActionResult Index()
        {
            var proc = Process.GetCurrentProcess();
            var dto = new JitDemoLegadoDto
            {
                Compilador = "RyuJIT (Compilacao em Tempo de Execucao)",
                MemoriaWorkingSetMb = Math.Round(proc.WorkingSet64 / (1024.0 * 1024.0), 2),
                CompilacaoJitAtiva = true,
                Serializador = "Newtonsoft.Json (Reflection Pesada em Runtime)",
                VersaoClr = Environment.Version.ToString()
            };

            return View(dto);
        }

        [HttpPost]
        public ActionResult Benchmark()
        {
            const int iteracoes = 100000;
            var sw = Stopwatch.StartNew();

            long soma = 0;
            for (int i = 0; i < iteracoes; i++)
            {
                soma += (i ^ (i << 2)) & 0xFFFFFF;
            }

            sw.Stop();
            var proc = Process.GetCurrentProcess();

            return Json(new
            {
                iteracoes,
                tempoTotalMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                memoriaMb = Math.Round(proc.WorkingSet64 / (1024.0 * 1024.0), 2)
            });
        }
    }
}
