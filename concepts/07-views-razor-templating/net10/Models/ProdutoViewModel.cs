using System.ComponentModel.DataAnnotations;

namespace ViewsAndRazorDemo.Models;

public class ProdutoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 60 caracteres.")]
    [Display(Name = "Nome do Produto")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(30, ErrorMessage = "A categoria deve ter no máximo 30 caracteres.")]
    [Display(Name = "Categoria")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0.50, 99999.00, ErrorMessage = "O preço deve ser maior que R$ 0,50.")]
    [Display(Name = "Preço Unitário (R$)")]
    public decimal Preco { get; set; }

    [Display(Name = "Item em Estoque?")]
    public bool EmEstoque { get; set; } = true;
}
