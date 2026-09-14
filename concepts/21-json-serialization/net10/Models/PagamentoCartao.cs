namespace JsonSerializationDemo.Models;

public class PagamentoCartao : Pagamento
{
    public string NumeroMascarado { get; set; } = string.Empty;
    public int Parcelas { get; set; }
    public string Bandeira { get; set; } = "Mastercard";
}
