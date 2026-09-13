using System.ComponentModel.DataAnnotations;

namespace SecurityBestPracticesDemo.Models;

/// <summary>
/// Modelo de requisição para transferência financeira.
/// Representa uma operação crítica que exige proteção contra Cross-Site Request Forgery (CSRF).
/// </summary>
public sealed class TransferenciaRequest
{
    [Required(ErrorMessage = "A conta de origem é obrigatória.")]
    public string ContaOrigem { get; set; } = string.Empty;

    [Required(ErrorMessage = "A conta de destino é obrigatória.")]
    public string ContaDestino { get; set; } = string.Empty;

    [Range(0.01, 100000.00, ErrorMessage = "O valor deve ser positivo e de até R$ 100.000,00.")]
    public decimal Valor { get; set; }

    public string Descricao { get; set; } = "Transferência via Internet Banking";
}
