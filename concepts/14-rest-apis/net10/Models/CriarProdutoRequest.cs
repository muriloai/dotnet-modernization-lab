using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models;

/// <summary>
/// Modelo de entrada para criação de novo produto com anotações de validação.
/// No .NET 10 com [ApiController], falhas nessas anotações disparam automaticamente
/// uma resposta 400 Bad Request no padrão RFC 7807 ProblemDetails.
/// </summary>
public sealed record CriarProdutoRequest(
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O SKU é obrigatório.")]
    [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "O SKU deve seguir o formato de três letras maiúsculas, hífen e quatro dígitos (ex: PRD-1234).")]
    string Sku,

    [Range(0.01, 1000000.00, ErrorMessage = "O preço deve ser maior que zero.")]
    decimal Preco,

    [Range(0, 100000, ErrorMessage = "A quantidade de estoque não pode ser negativa.")]
    int Estoque,

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "A categoria deve ter entre 2 e 50 caracteres.")]
    string Categoria
);
