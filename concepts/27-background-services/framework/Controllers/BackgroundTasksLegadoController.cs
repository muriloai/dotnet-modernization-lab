using System;
using System.Linq;
using System.Web.Mvc;
using BackgroundServicesDemo.Services;

namespace BackgroundServicesDemo.Controllers
{
    public class BackgroundTasksLegadoController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Enfileirar(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
            {
                return Json(new { sucesso = false, mensagem = "Descricao e obrigatoria" });
            }

            FilaProcessamentoLegada.EnfileirarComQueueBackgroundWorkItem(descricao);
            return Json(new { sucesso = true });
        }

        [HttpGet]
        public ActionResult ObterStatus()
        {
            var tarefas = FilaProcessamentoLegada.ObterTarefas();
            var totalEnfileiradas = tarefas.Count;
            var totalEmProcessamento = tarefas.Count(t => t.Status == "Em Processamento");
            var totalConcluidas = tarefas.Count(t => t.Status == "Concluido");

            return Json(new
            {
                totalEnfileiradas,
                totalEmProcessamento,
                totalConcluidas,
                ticksTimer = FilaProcessamentoLegada.TotalTicks,
                ultimoTick = FilaProcessamentoLegada.UltimoTick.ToString("HH:mm:ss"),
                memoriaBytes = GC.GetTotalMemory(false),
                tarefas
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
