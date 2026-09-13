using System.ComponentModel.DataAnnotations;

namespace SecurityBestPracticesDemo.Models
{
    /// <summary>
    /// Modelo de transferência financeira para demonstração no MVC e Web API legados.
    /// </summary>
    public class TransferenciaModel
    {
        [Required(ErrorMessage = "A conta de origem é obrigatória.")]
        [Display(Name = "Conta de Origem")]
        public string ContaOrigem { get; set; }

        [Required(ErrorMessage = "A conta de destino é obrigatória.")]
        [Display(Name = "Conta de Destino")]
        public string ContaDestino { get; set; }

        [Range(0.01, 100000.00, ErrorMessage = "O valor deve ser positivo e de até R$ 100.000,00.")]
        [Display(Name = "Valor (R$)")]
        public decimal Valor { get; set; }

        [Display(Name = "Descrição da Operação")]
        public string Descricao { get; set; }
    }
}
