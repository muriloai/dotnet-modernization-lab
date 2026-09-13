using System.ComponentModel.DataAnnotations;

namespace ControllersAndActionsDemo.Models;

/// <summary>
/// Modelo de entrada utilizado na action POST com validações declarativas.
/// No .NET 10, o atributo [ApiController] valida essas anotações automaticamente
/// e responde com HTTP 400 ProblemDetails caso alguma regra seja violada.
/// </summary>
public sealed class CriarProdutoRequest
{
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, 100000.00, ErrorMessage = "O preço deve ser maior que zero e até R$ 100.000,00.")]
    public decimal Preco { get; set; }

    public bool EmEstoque { get; set; } = true;
}
