using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models;

/// <summary>
/// Modelo de entrada para atualização completa (PUT) de um produto existente.
/// </summary>
public sealed record AtualizarProdutoRequest(
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
    string Nome,

    [Range(0.01, 1000000.00, ErrorMessage = "O preço deve ser maior que zero.")]
    decimal Preco,

    [Range(0, 100000, ErrorMessage = "A quantidade de estoque não pode ser negativa.")]
    int Estoque,

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "A categoria deve ter entre 2 e 50 caracteres.")]
    string Categoria,

    bool Ativo
);
