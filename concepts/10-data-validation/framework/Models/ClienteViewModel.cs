using System.ComponentModel.DataAnnotations;
using DataValidationDemo.Models.Validations;

namespace DataValidationDemo.Models
{
    /// <summary>
    /// ViewModel para formulário de cadastro no ASP.NET MVC 5.
    /// No MVC legado, o formulário submete via POST form-url-encoded e a Controller retorna View(model) em caso de erro.
    /// </summary>
    public class ClienteViewModel
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não possui um formato válido.")]
        [Display(Name = "E-mail Principal")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [CpfValidation(ErrorMessage = "O CPF do cliente informado não é válido.")]
        [Display(Name = "CPF")]
        public string Cpf { get; set; }

        [Range(18, 120, ErrorMessage = "A idade do titular deve estar entre 18 e 120 anos.")]
        [Display(Name = "Idade")]
        public int Idade { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "A renda mensal deve ser um valor positivo de até R$ 1.000.000,00.")]
        [Display(Name = "Renda Mensal (R$)")]
        public decimal RendaMensal { get; set; }

        [Range(0.00, 5000000.00, ErrorMessage = "O limite de crédito solicitado deve estar entre R$ 0,00 e R$ 5.000.000,00.")]
        [Display(Name = "Limite Solicitado (R$)")]
        public decimal LimiteCreditoSolicitado { get; set; }
    }
}
