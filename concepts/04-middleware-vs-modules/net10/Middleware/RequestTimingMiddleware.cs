using System.Diagnostics;

namespace MiddlewareVsModulesDemo.Middleware;

/// <summary>
/// Middleware que mede o tempo total de processamento da requisição HTTP.
/// Demonstra o fluxo bidirecional: inicia o cronômetro antes do 'await next(context)'
/// e calcula o tempo total na volta, registrando o cabeçalho via Response.OnStarting.
/// </summary>
public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Registra um callback para injetar o header antes do envio do corpo da resposta
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            context.Response.Headers["X-Response-Time-Ms"] = elapsedMs.ToString();
            logger.LogInformation("Requisição {Method} {Path} processada em {Elapsed}ms.",
                context.Request.Method, context.Request.Path, elapsedMs);

            return Task.CompletedTask;
        });

        // Passa a execução para o próximo middleware no pipeline
        await next(context);
    }
}
