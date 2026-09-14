using PedidosCqrsDemo.Features.Pedidos.DTOs;
using PedidosCqrsDemo.Infrastructure.Mediator;
using PedidosCqrsDemo.Infrastructure.Persistence;

namespace PedidosCqrsDemo.Features.Pedidos.Queries;

public record ObterPedidosQuery() : IRequest<IReadOnlyList<PedidoDetalheDto>>;

public class ObterPedidosHandler : IRequestHandler<ObterPedidosQuery, IReadOnlyList<PedidoDetalheDto>>
{
    private readonly PedidoRepositoryEmMemoria _repository;
    private readonly ILogger<ObterPedidosHandler> _logger;

    public ObterPedidosHandler(PedidoRepositoryEmMemoria repository, ILogger<ObterPedidosHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PedidoDetalheDto>> Handle(ObterPedidosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executando Query: ObterPedidos");
        await Task.Delay(50, cancellationToken);
        return _repository.ObterTodos();
    }
}
