namespace MiddlewareVsModulesDemo.Middleware;

/// <summary>
/// Middleware que adiciona cabeçalhos informativos à resposta HTTP.
/// Demonstra a facilidade de manipular cabeçalhos diretamente através do HttpContext.
/// </summary>
public sealed class CustomHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Execution-Engine"] = "DotNet-10-Kestrel";
            context.Response.Headers["X-Pipeline-Type"] = "ASP.NET-Core-Middleware";
            return Task.CompletedTask;
        });

        await next(context);
    }
}
