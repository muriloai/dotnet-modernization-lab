using System;

namespace RestApisDemo.Models
{
    /// <summary>
    /// Representação pública serializada retornada pela Web API 2.
    /// </summary>
    public class ProdutoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Sku { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
        public string Categoria { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
