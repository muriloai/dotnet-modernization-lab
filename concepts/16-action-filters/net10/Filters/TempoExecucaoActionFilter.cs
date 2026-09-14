using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFiltersDemo.Filters;

/// <summary>
/// Filtro assíncrono moderno (IAsyncActionFilter) que mede o tempo de execução da Action
/// e injeta o resultado no cabeçalho HTTP de resposta 'X-Tempo-Execucao-Ms'.
/// Pode ser aplicado via [TypeFilter(typeof(TempoExecucaoActionFilter))] ou globalmente.
/// </summary>
public sealed class TempoExecucaoActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Código executado ANTES da Action
        var stopwatch = Stopwatch.StartNew();

        // 2. Execução da Action através do delegate next()
        var executedContext = await next();

        // 3. Código executado DEPOIS da Action
        stopwatch.Stop();
        var tempoMs = stopwatch.ElapsedMilliseconds;

        // Injeta cabeçalho customizado na resposta HTTP se ela ainda não tiver sido iniciada
        if (!context.HttpContext.Response.HasStarted)
        {
            context.HttpContext.Response.Headers.TryAdd("X-Tempo-Execucao-Ms", tempoMs.ToString());
            context.HttpContext.Response.Headers.TryAdd("X-Filtro-Auditoria", "TempoExecucaoActionFilter (.NET 10)");
        }
    }
}
