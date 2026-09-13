using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkDemo.Models;

public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0.10, 99999.00, ErrorMessage = "O preço deve ser superior a R$ 0,10.")]
    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    [Range(0, 10000, ErrorMessage = "O estoque não pode ser negativo.")]
    public int Estoque { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}
