namespace PedidosCqrsDemo.Features.Pedidos.DTOs;

public record CriarPedidoRequest(
    string Cliente,
    string Produto,
    decimal ValorUnitario,
    int Quantidade
);

public record PedidoDetalheDto(
    Guid Id,
    string Cliente,
    string Produto,
    decimal ValorUnitario,
    int Quantidade,
    decimal ValorTotal,
    string Status,
    DateTime CriadoEm
);
