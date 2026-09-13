using System;

namespace RestApisDemo.Models
{
    /// <summary>
    /// Entidade de domínio representando o produto no repositório legado.
    /// </summary>
    public class Produto
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
