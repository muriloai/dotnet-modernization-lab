using System.Collections.Concurrent;

namespace PedidosCqrsDemo.Infrastructure.Mediator;

public record TraceItem(DateTime Timestamp, string Estagio, string Mensagem);

public class TraceLogService
{
    private readonly ConcurrentQueue<TraceItem> _traces = new();

    public void Adicionar(string estagio, string mensagem)
    {
        _traces.Enqueue(new TraceItem(DateTime.UtcNow, estagio, mensagem));
        while (_traces.Count > 50)
        {
            _traces.TryDequeue(out _);
        }
    }

    public IReadOnlyList<TraceItem> ObterTraces()
    {
        return _traces.Reverse().ToList();
    }
}

public class MediatorDispatcher : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public MediatorDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"Nenhum handler registrado para a requisicao: {requestType.Name}");
        }

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = (_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(behaviorType)) as IEnumerable<object> ?? Enumerable.Empty<object>())
            .Cast<dynamic>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResponse> pipeline = () =>
        {
            dynamic dynamicHandler = handler;
            return (Task<TResponse>)dynamicHandler.Handle((dynamic)request, cancellationToken);
        };

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            var currentBehavior = behavior;
            pipeline = () => currentBehavior.Handle((dynamic)request, next, cancellationToken);
        }

        return await pipeline();
    }
}
