namespace JsonSerializationDemo.Models;

public class PagamentoPix : Pagamento
{
    public string ChavePix { get; set; } = string.Empty;
    public string IdentificadorTransacao { get; set; } = string.Empty;
}
