using System;
using System.Collections.Generic;

namespace RepositoryPatternDemo.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Status { get; set; }
        public decimal ValorTotal { get; set; }

        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }

        public virtual ICollection<ItemPedido> Itens { get; set; }

        public Pedido()
        {
            DataCriacao = DateTime.UtcNow;
            Status = "Pendente";
            Itens = new HashSet<ItemPedido>();
        }
    }
}
