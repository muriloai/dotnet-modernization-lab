using System.Diagnostics;
using System.Text.Json;
using PedidosCqrsDemo.Infrastructure.Mediator;

namespace PedidosCqrsDemo.Infrastructure.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly TraceLogService _traceLog;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, TraceLogService traceLog)
    {
        _logger = logger;
        _traceLog = traceLog;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[Mediator Pipeline] Iniciando requisicao: {RequestName}", requestName);
        _traceLog.Adicionar("LoggingBehavior: Pre-execucao", $"Iniciando {requestName}");

        var response = await next();

        _logger.LogInformation("[Mediator Pipeline] Concluida requisicao: {RequestName}", requestName);
        _traceLog.Adicionar("LoggingBehavior: Pos-execucao", $"Finalizado {requestName} com sucesso");

        return response;
    }
}

public class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TimingBehavior<TRequest, TResponse>> _logger;
    private readonly TraceLogService _traceLog;

    public TimingBehavior(ILogger<TimingBehavior<TRequest, TResponse>> logger, TraceLogService traceLog)
    {
        _logger = logger;
        _traceLog = traceLog;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _traceLog.Adicionar("TimingBehavior: Cronometro", $"Iniciando medicao para {requestName}");

        var response = await next();

        sw.Stop();
        _logger.LogInformation("[Mediator Pipeline] Requisicao {RequestName} executada em {ElapsedMs} ms", requestName, sw.ElapsedMilliseconds);
        _traceLog.Adicionar("TimingBehavior: Metrica", $"Requisicao {requestName} finalizada em {sw.ElapsedMilliseconds} ms ({sw.ElapsedTicks} ticks)");

        return response;
    }
}
