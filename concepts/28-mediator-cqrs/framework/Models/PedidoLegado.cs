using System;

namespace PedidosCqrsDemo.Models
{
    public class PedidoLegado
    {
        public Guid Id { get; set; }
        public string Cliente { get; set; }
        public string Produto { get; set; }
        public decimal ValorUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataFaturamento { get; set; }
        public string ObservacoesInternas { get; set; }
        public bool IsDeletado { get; set; }

        public PedidoLegado()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            Status = "Pendente";
        }
    }
}
