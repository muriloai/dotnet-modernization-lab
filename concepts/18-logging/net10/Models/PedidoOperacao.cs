namespace LoggingDemo.Models;

public class PedidoOperacao
{
    public int PedidoId { get; set; }
    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public int QuantidadeItens { get; set; }
    public string Canal { get; set; } = "Web";
}
