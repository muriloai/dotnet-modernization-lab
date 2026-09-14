using System.Text.Json.Serialization;

namespace JsonSerializationDemo.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$tipo")]
[JsonDerivedType(typeof(PagamentoPix), "pix")]
[JsonDerivedType(typeof(PagamentoCartao), "cartao")]
public abstract class Pagamento
{
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
