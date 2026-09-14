namespace LoggingDemo.Models
{
    public class PedidoLegado
    {
        public int PedidoId { get; set; }
        public string ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public decimal ValorTotal { get; set; }
        public int QuantidadeItens { get; set; }
        public string Canal { get; set; }

        public PedidoLegado()
        {
            PedidoId = 101;
            ClienteId = "CLI-8890";
            ClienteNome = "Empresa Alpha Ltda";
            ValorTotal = 1450.75m;
            QuantidadeItens = 4;
            Canal = "E-Commerce B2B";
        }
    }
}
