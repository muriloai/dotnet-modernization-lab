using System.Collections.Generic;

namespace CSharpEvolutionDemo.Models;

public record ItemPedido(string Produto, int Quantidade, decimal PrecoUnitario);

public record Pedido(
    int Id,
    Cliente Cliente,
    List<ItemPedido> Itens,
    decimal ValorTotal,
    string Status
);
