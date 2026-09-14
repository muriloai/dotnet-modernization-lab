using PedidosCqrsDemo.Features.Pedidos.DTOs;
using PedidosCqrsDemo.Infrastructure.Mediator;
using PedidosCqrsDemo.Infrastructure.Persistence;

namespace PedidosCqrsDemo.Features.Pedidos.Commands;

public record CriarPedidoCommand(
    string Cliente,
    string Produto,
    decimal ValorUnitario,
    int Quantidade
) : IRequest<PedidoDetalheDto>;

public class CriarPedidoHandler : IRequestHandler<CriarPedidoCommand, PedidoDetalheDto>
{
    private readonly PedidoRepositoryEmMemoria _repository;
    private readonly ILogger<CriarPedidoHandler> _logger;

    public CriarPedidoHandler(PedidoRepositoryEmMemoria repository, ILogger<CriarPedidoHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PedidoDetalheDto> Handle(CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executando Command: CriarPedido para o cliente {Cliente}", request.Cliente);

        // Simulando pequena operacao assincrona
        await Task.Delay(150, cancellationToken);

        var total = request.ValorUnitario * request.Quantidade;
        var pedido = new PedidoDetalheDto(
            Id: Guid.NewGuid(),
            Cliente: request.Cliente.Trim(),
            Produto: request.Produto.Trim(),
            ValorUnitario: request.ValorUnitario,
            Quantidade: request.Quantidade,
            ValorTotal: total,
            Status: "Pendente",
            CriadoEm: DateTime.UtcNow
        );

        _repository.Inserir(pedido);
        return pedido;
    }
}
