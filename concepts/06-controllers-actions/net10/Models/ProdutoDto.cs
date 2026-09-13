namespace ControllersAndActionsDemo.Models;

/// <summary>
/// Record representando o Produto devolvido pelas Actions do Controller e Minimal APIs.
/// </summary>
public record ProdutoDto(int Id, string Nome, string Categoria, decimal Preco, bool EmEstoque);
