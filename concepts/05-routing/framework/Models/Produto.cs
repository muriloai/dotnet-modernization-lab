namespace RoutingDemo.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public decimal Preco { get; set; }
        public bool EmEstoque { get; set; }

        public Produto(int id, string nome, string categoria, decimal preco, bool emEstoque)
        {
            Id = id;
            Nome = nome;
            Categoria = categoria;
            Preco = preco;
            EmEstoque = emEstoque;
        }
    }
}
