using ErrorHandlingDemo.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ErrorHandlingDemo.Handlers;

public class RegraNegocioExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<RegraNegocioExceptionHandler> _logger;

    public RegraNegocioExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<RegraNegocioExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RegraNegocioException regraEx)
        {
            return false;
        }

        _logger.LogWarning(
            "Regra de negócio violada. Código: {CodigoRegra}, Mensagem: {Mensagem}",
            regraEx.CodigoRegra,
            regraEx.Message);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Regra de negócio violada",
            Detail = regraEx.Message,
            Type = "https://httpstatuses.io/422",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["codigoRegra"] = regraEx.CodigoRegra;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
