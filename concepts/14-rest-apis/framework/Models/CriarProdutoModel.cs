using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models
{
    /// <summary>
    /// Modelo de entrada para criação no ASP.NET Web API 2.
    /// Diferente do ASP.NET Core, a validação não era automática:
    /// o desenvolvedor precisava checar explicitamente 'if (!ModelState.IsValid) return BadRequest(ModelState);'.
    /// </summary>
    public class CriarProdutoModel
    {
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O SKU é obrigatório.")]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "O SKU deve seguir o formato de três letras maiúsculas, hífen e quatro dígitos (ex: PRD-1234).")]
        public string Sku { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [Range(0, 100000, ErrorMessage = "A quantidade de estoque não pode ser negativa.")]
        public int Estoque { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "A categoria deve ter entre 2 e 50 caracteres.")]
        public string Categoria { get; set; }
    }
}
