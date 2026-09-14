namespace JsonSerializationDemo.Models
{
    public class ProdutoLegado
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public string Categoria { get; set; }
        public int Estoque { get; set; }

        public ProdutoLegado()
        {
        }

        public ProdutoLegado(int id, string nome, decimal preco, string categoria, int estoque)
        {
            Id = id;
            Nome = nome;
            Preco = preco;
            Categoria = categoria;
            Estoque = estoque;
        }
    }
}
