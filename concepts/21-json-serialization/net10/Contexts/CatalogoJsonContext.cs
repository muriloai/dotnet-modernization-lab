using System.Text.Json.Serialization;
using JsonSerializationDemo.Models;

namespace JsonSerializationDemo.Contexts;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<ProdutoCatalogo>))]
[JsonSerializable(typeof(ProdutoCatalogo))]
[JsonSerializable(typeof(Pagamento))]
[JsonSerializable(typeof(PagamentoPix))]
[JsonSerializable(typeof(PagamentoCartao))]
[JsonSerializable(typeof(MetricasSerializacao))]
public partial class CatalogoJsonContext : JsonSerializerContext
{
}
