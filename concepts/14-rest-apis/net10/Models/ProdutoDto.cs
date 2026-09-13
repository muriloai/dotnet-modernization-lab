namespace RestApisDemo.Models;

/// <summary>
/// DTO de saída expondo os dados públicos do produto de forma imutável.
/// </summary>
public sealed record ProdutoDto(
    int Id,
    string Nome,
    string Sku,
    decimal Preco,
    int Estoque,
    string Categoria,
    bool Ativo,
    DateTime DataCadastro
);
