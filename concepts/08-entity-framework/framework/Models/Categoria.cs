using System.Collections.Generic;

namespace EntityFrameworkDemo.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }

        public virtual ICollection<Produto> Produtos { get; set; }

        public Categoria()
        {
            Produtos = new HashSet<Produto>();
        }
    }
}
