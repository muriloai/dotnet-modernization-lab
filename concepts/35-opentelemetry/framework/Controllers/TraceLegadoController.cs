using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Mvc;
using TraceLegadoDemo.Models;

namespace TraceLegadoDemo.Controllers
{
    public class TraceLegadoController : Controller
    {
        private static readonly TraceSource _traceSource = new TraceSource("TraceLegadoSource");

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProcessarPedidoLegado(PedidoLegadoRequest request)
        {
            var sw = Stopwatch.StartNew();
            var logs = new List<LogEntryLegado>();

            // No legado, nao existia padrao W3C TraceContext. Cada dev inventava seu CorrelationId
            var correlationId = Guid.NewGuid().ToString("D");
            var pedidoId = "LEG-" + DateTime.UtcNow.Ticks.ToString().Substring(10);

            // Log 1: Inicio
            _traceSource.TraceEvent(TraceEventType.Information, 1001, "[{0}] Inicio do processamento do pedido {1}", correlationId, pedidoId);
            logs.Add(new LogEntryLegado
            {
                Timestamp = DateTime.UtcNow,
                Nivel = "Information",
                Mensagem = string.Format("[{0}] Inicio do pedido para o cliente {1}", correlationId, request != null ? request.Cliente : "Padrao"),
                ThreadId = Thread.CurrentThread.ManagedThreadId.ToString()
            });

            // Simula etapa de pagamento com Trace.WriteLine classico
            Thread.Sleep(120);
            Trace.WriteLine(string.Format("[{0}] Pagamento aprovado via gateway legado", correlationId));
            logs.Add(new LogEntryLegado
            {
                Timestamp = DateTime.UtcNow,
                Nivel = "Verbose",
                Mensagem = string.Format("[{0}] Gateway de pagamento respondeu 200 OK (sem propagacao de headers W3C)", correlationId),
                ThreadId = Thread.CurrentThread.ManagedThreadId.ToString()
            });

            // Simula etapa de estoque
            Thread.Sleep(80);
            logs.Add(new LogEntryLegado
            {
                Timestamp = DateTime.UtcNow,
                Nivel = "Information",
                Mensagem = string.Format("[{0}] Estoque atualizado no SQL Server (sem spans distribuidos)", correlationId),
                ThreadId = Thread.CurrentThread.ManagedThreadId.ToString()
            });

            sw.Stop();

            var response = new PedidoLegadoResponse
            {
                PedidoId = pedidoId,
                CorrelationId = correlationId,
                TempoExecucaoMs = sw.ElapsedMilliseconds,
                LogsGerados = logs,
                MensagemDiagnostico = "No .NET Framework 4.8.1, os logs sao salvos em texto puro ou visualizadores do Windows. Nao ha W3C traceparent e nao ha arvore hierarquica de spans (waterfall)."
            };

            return Json(response);
        }
    }
}
