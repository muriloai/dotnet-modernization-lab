namespace RepositoryPatternDemo.Models.DTOs;

public record PedidoResumoDto(
    int Id,
    string NumeroPedido,
    string NomeCliente,
    DateTime DataCriacao,
    string Status,
    decimal ValorTotal,
    int QuantidadeItens
);
