namespace RoutingDemo.Models;

/// <summary>
/// Modelo de domínio que representa um produto no laboratório de roteamento.
/// </summary>
public record Produto(int Id, string Nome, string Categoria, decimal Preco, bool EmEstoque);
