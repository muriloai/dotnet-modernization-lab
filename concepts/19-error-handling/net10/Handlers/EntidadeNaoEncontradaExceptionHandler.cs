using ErrorHandlingDemo.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ErrorHandlingDemo.Handlers;

public class EntidadeNaoEncontradaExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<EntidadeNaoEncontradaExceptionHandler> _logger;

    public EntidadeNaoEncontradaExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<EntidadeNaoEncontradaExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not EntidadeNaoEncontradaException notFoundEx)
        {
            return false;
        }

        _logger.LogWarning(
            "Recurso não encontrado: Entidade '{Entidade}', Chave '{Chave}'",
            notFoundEx.Entidade,
            notFoundEx.Chave);

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso não localizado",
            Detail = notFoundEx.Message,
            Type = "https://httpstatuses.io/404",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["entidade"] = notFoundEx.Entidade;
        problemDetails.Extensions["chave"] = notFoundEx.Chave?.ToString();

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
