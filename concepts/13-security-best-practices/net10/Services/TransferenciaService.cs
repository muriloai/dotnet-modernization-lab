using System.Collections.Concurrent;
using System.Text.Json;
using SecurityBestPracticesDemo.Models;

namespace SecurityBestPracticesDemo.Services;

/// <summary>
/// Implementação do serviço de transferências com serialização segura via System.Text.Json.
/// Substitui o BinaryFormatter obsoleto e vulnerável do ecossistema legado.
/// </summary>
public sealed class TransferenciaService : ITransferenciaService
{
    private static readonly ConcurrentBag<TransacaoRegistro> Transacoes = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        MaxDepth = 8 // Impede ataques de profundidade excessiva de payload (DoS)
    };

    static TransferenciaService()
    {
        Transacoes.Add(new TransacaoRegistro(
            Id: Guid.NewGuid(),
            ContaOrigem: "001-98765-4",
            ContaDestino: "237-12345-6",
            Valor: 1250.00m,
            Descricao: "Pagamento de fornecedores",
            DataUtc: DateTime.UtcNow.AddHours(-3),
            ProtocoloSeguranca: "System.Text.Json (Seguro)"
        ));
    }

    public Task<TransacaoRegistro> ExecutarTransferenciaAsync(TransferenciaRequest request)
    {
        var transacao = new TransacaoRegistro(
            Id: Guid.NewGuid(),
            ContaOrigem: request.ContaOrigem,
            ContaDestino: request.ContaDestino,
            Valor: request.Valor,
            Descricao: request.Descricao,
            DataUtc: DateTime.UtcNow,
            ProtocoloSeguranca: "System.Text.Json (Seguro)"
        );

        Transacoes.Add(transacao);
        return Task.FromResult(transacao);
    }

    public IEnumerable<TransacaoRegistro> ListarTransacoes() => Transacoes.OrderByDescending(t => t.DataUtc);

    public string SerializarTransacaoSegura(TransacaoRegistro transacao)
    {
        return JsonSerializer.Serialize(transacao, JsonOptions);
    }

    public TransacaoRegistro? DesserializarTransacaoSegura(string json)
    {
        return JsonSerializer.Deserialize<TransacaoRegistro>(json, JsonOptions);
    }
}
