namespace LoggingDemo.Models;

public class RequisicaoProcessamento
{
    public int PedidoId { get; set; } = 101;
    public string ClienteId { get; set; } = "CLI-8890";
    public string ClienteNome { get; set; } = "Empresa Alpha Ltda";
    public decimal ValorTotal { get; set; } = 1450.75m;
    public int QuantidadeItens { get; set; } = 4;
    public string Canal { get; set; } = "E-Commerce B2B";
    public bool SimularErro { get; set; }
    public bool SimularAlertaEstoque { get; set; }
    public bool UsarLoggerOtimizado { get; set; }
}
