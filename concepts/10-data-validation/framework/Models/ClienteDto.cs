using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataValidationDemo.Models.Validations;

namespace DataValidationDemo.Models
{
    /// <summary>
    /// DTO para recepção de requisições na Web API clássica.
    /// Demonstra a validação via DataAnnotations e IValidatableObject no .NET Framework 4.8.1.
    /// </summary>
    public class ClienteDto : IValidatableObject
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não possui um formato válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [CpfValidation(ErrorMessage = "O CPF do cliente informado não é válido.")]
        public string Cpf { get; set; }

        [Range(18, 120, ErrorMessage = "A idade do titular deve estar entre 18 e 120 anos.")]
        public int Idade { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "A renda mensal deve ser um valor positivo de até R$ 1.000.000,00.")]
        public decimal RendaMensal { get; set; }

        [Range(0.00, 5000000.00, ErrorMessage = "O limite de crédito solicitado deve estar entre R$ 0,00 e R$ 5.000.000,00.")]
        public decimal LimiteCreditoSolicitado { get; set; }

        public bool PossuiRepresentante { get; set; }

        public string NomeRepresentante { get; set; }

        [CpfValidation(ErrorMessage = "O CPF do representante informado não é válido.")]
        public string CpfRepresentante { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PossuiRepresentante)
            {
                if (string.IsNullOrWhiteSpace(NomeRepresentante))
                {
                    yield return new ValidationResult(
                        "O nome do representante legal é obrigatório quando a opção possui representante estiver marcada.",
                        new[] { "NomeRepresentante" }
                    );
                }

                if (string.IsNullOrWhiteSpace(CpfRepresentante))
                {
                    yield return new ValidationResult(
                        "O CPF do representante legal é obrigatório quando a opção possui representante estiver marcada.",
                        new[] { "CpfRepresentante" }
                    );
                }
            }

            if (RendaMensal > 0 && LimiteCreditoSolicitado > (RendaMensal * 5))
            {
                yield return new ValidationResult(
                    string.Format("O limite de crédito solicitado (R$ {0:N2}) não pode exceder 5 vezes a renda mensal comprovada (R$ {1:N2}).", LimiteCreditoSolicitado, RendaMensal),
                    new[] { "LimiteCreditoSolicitado" }
                );
            }
        }
    }
}
