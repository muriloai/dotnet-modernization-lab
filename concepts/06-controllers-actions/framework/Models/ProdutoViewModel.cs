using System.ComponentModel.DataAnnotations;

namespace ControllersAndActionsDemo.Models
{
    public class ProdutoViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string Categoria { get; set; }

        [Range(0.01, 100000.00, ErrorMessage = "O preço deve ser positivo.")]
        public decimal Preco { get; set; }

        public bool EmEstoque { get; set; } = true;
    }
}
