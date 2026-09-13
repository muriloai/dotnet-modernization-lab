using System.Collections.Generic;

namespace RepositoryPatternDemo.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }

        public virtual ICollection<Pedido> Pedidos { get; set; }

        public Cliente()
        {
            Pedidos = new HashSet<Pedido>();
        }
    }
}
