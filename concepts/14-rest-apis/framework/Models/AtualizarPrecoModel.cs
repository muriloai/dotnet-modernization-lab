using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models
{
    /// <summary>
    /// Modelo de entrada para atualização parcial do preço.
    /// </summary>
    public class AtualizarPrecoModel
    {
        [Range(0.01, 1000000.00, ErrorMessage = "O novo preço deve ser maior que zero.")]
        public decimal NovoPreco { get; set; }
    }
}
