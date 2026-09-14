using System.Diagnostics;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ActionFiltersDemo.Filters
{
    /// <summary>
    /// Filtro da Web API 2 herdando de System.Web.Http.Filters.ActionFilterAttribute.
    /// Diferente do .NET 10 (que possui um método assíncrono único com next()),
    /// o modelo legado dividia a execução em dois métodos síncronos separados,
    /// exigindo o uso de Request.Properties para compartilhar dados entre o início e o fim.
    /// </summary>
    public class MedirTempoExecucaoFilter : ActionFilterAttribute
    {
        private const string StopwatchKey = "CronometroExecucaoFiltro";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Salva o Stopwatch no dicionário temporário de propriedades da requisição
            actionContext.Request.Properties[StopwatchKey] = Stopwatch.StartNew();
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Request.Properties.TryGetValue(StopwatchKey, out var obj) && obj is Stopwatch sw)
            {
                sw.Stop();
                if (actionExecutedContext.Response != null)
                {
                    actionExecutedContext.Response.Headers.Add("X-Tempo-Execucao-Ms", sw.ElapsedMilliseconds.ToString());
                    actionExecutedContext.Response.Headers.Add("X-Filtro-Auditoria", "MedirTempoExecucaoFilter (.NET Framework 4.8.1)");
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
