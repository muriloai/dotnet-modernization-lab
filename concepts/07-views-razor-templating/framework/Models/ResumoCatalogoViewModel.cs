using System;

namespace ViewsAndRazorDemo.Models
{
    public class ResumoCatalogoViewModel
    {
        public int TotalProdutos { get; set; }
        public decimal ValorTotalEstoque { get; set; }
        public int TotalCategoriasDistintas { get; set; }
        public int ItensEmEstoque { get; set; }
        public DateTime CalculadoEm { get; set; } = DateTime.Now;
    }
}
