using ErrorHandlingDemo.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ErrorHandlingDemo.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exceção não tratada capturada pelo GlobalExceptionHandler na rota {Rota}",
            httpContext.Request.Path);

        int statusCode;
        string titulo;
        string detalhe;
        string tipo;

        if (exception is IntegracaoExternaException integracaoEx)
        {
            statusCode = StatusCodes.Status502BadGateway;
            titulo = "Falha na comunicação com serviço externo";
            detalhe = $"O serviço parceiro '{integracaoEx.ServicoDestino}' não respondeu adequadamente.";
            tipo = "https://httpstatuses.io/502";
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            titulo = "Erro interno no servidor";
            detalhe = _environment.IsDevelopment()
                ? exception.Message
                : "Ocorreu uma falha inesperada durante o processamento de sua solicitação.";
            tipo = "https://httpstatuses.io/500";
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = detalhe,
            Type = tipo,
            Instance = httpContext.Request.Path
        };

        if (exception is IntegracaoExternaException integracao)
        {
            problemDetails.Extensions["servicoDestino"] = integracao.ServicoDestino;
        }

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
