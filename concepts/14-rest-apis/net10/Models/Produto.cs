namespace RestApisDemo.Models;

/// <summary>
/// Entidade de domínio representando um produto no catálogo.
/// </summary>
public sealed class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}
