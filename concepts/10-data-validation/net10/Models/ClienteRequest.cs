using System.ComponentModel.DataAnnotations;
using DataValidationDemo.Models.Validations;

namespace DataValidationDemo.Models;

/// <summary>
/// Modelo de requisição para cadastro de cliente.
/// Demonstra o uso de DataAnnotations padrão, validador customizado e validação composta via IValidatableObject.
/// </summary>
public sealed class ClienteRequest : IValidatableObject
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não possui um formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [CpfValidation(ErrorMessage = "O CPF do cliente informado não é válido.")]
    public string Cpf { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "A idade do titular deve estar entre 18 e 120 anos.")]
    public int Idade { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "A renda mensal deve ser um valor positivo de até R$ 1.000.000,00.")]
    public decimal RendaMensal { get; set; }

    [Range(0.00, 5000000.00, ErrorMessage = "O limite de crédito solicitado deve estar entre R$ 0,00 e R$ 5.000.000,00.")]
    public decimal LimiteCreditoSolicitado { get; set; }

    public bool PossuiRepresentante { get; set; }

    public string? NomeRepresentante { get; set; }

    [CpfValidation(ErrorMessage = "O CPF do representante informado não é válido.")]
    public string? CpfRepresentante { get; set; }

    /// <summary>
    /// Validação composta entre múltiplos campos (Cross-Property Validation).
    /// Executada automaticamente pelo pipeline do ASP.NET Core após as validações de atributo passarem.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Regra 1: Validação condicional de representante legal
        if (PossuiRepresentante)
        {
            if (string.IsNullOrWhiteSpace(NomeRepresentante))
            {
                yield return new ValidationResult(
                    "O nome do representante legal é obrigatório quando a opção possui representante estiver marcada.",
                    new[] { nameof(NomeRepresentante) }
                );
            }

            if (string.IsNullOrWhiteSpace(CpfRepresentante))
            {
                yield return new ValidationResult(
                    "O CPF do representante legal é obrigatório quando a opção possui representante estiver marcada.",
                    new[] { nameof(CpfRepresentante) }
                );
            }
        }

        // Regra 2: Cruzamento financeiro entre limite solicitado e renda mensal
        if (RendaMensal > 0 && LimiteCreditoSolicitado > (RendaMensal * 5))
        {
            yield return new ValidationResult(
                $"O limite de crédito solicitado (R$ {LimiteCreditoSolicitado:N2}) não pode exceder 5 vezes a renda mensal comprovada (R$ {RendaMensal:N2}).",
                new[] { nameof(LimiteCreditoSolicitado) }
            );
        }
    }
}
