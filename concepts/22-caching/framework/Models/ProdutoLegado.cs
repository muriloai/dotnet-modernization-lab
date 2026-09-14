using System;

namespace CachingDemo.Models
{
    public class ProdutoLegado
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataHoraCriacao { get; set; }

        public ProdutoLegado()
        {
            DataHoraCriacao = DateTime.Now;
        }

        public ProdutoLegado(int id, string nome, string categoria, decimal preco)
        {
            Id = id;
            Nome = nome;
            Categoria = categoria;
            Preco = preco;
            DataHoraCriacao = DateTime.Now;
        }
    }
}
