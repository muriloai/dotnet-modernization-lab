using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models
{
    /// <summary>
    /// Modelo de entrada para atualização completa (PUT) de um produto existente.
    /// </summary>
    public class AtualizarProdutoModel
    {
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve conter entre 3 e 100 caracteres.")]
        public string Nome { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [Range(0, 100000, ErrorMessage = "A quantidade de estoque não pode ser negativa.")]
        public int Estoque { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "A categoria deve ter entre 2 e 50 caracteres.")]
        public string Categoria { get; set; }

        public bool Ativo { get; set; }
    }
}
