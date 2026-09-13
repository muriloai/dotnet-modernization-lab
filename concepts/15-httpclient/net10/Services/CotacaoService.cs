using System.Diagnostics;
using System.Net.Http.Json;
using HttpClientDemo.Models;

namespace HttpClientDemo.Services;

/// <summary>
/// Implementação de Typed Client no .NET 10.
/// O HttpClient recebido no construtor já possui BaseAddress e cabeçalhos pré-configurados
/// via builder.Services.AddHttpClient de forma desacoplada no Program.cs.
/// </summary>
public sealed class CotacaoService : ICotacaoService
{
    private readonly HttpClient _httpClient;

    public CotacaoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MetricaConexao> ObterCotacaoTypedClientAsync(string moeda, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var cotacao = await _httpClient.GetFromJsonAsync<CotacaoMoeda>($"api/mock-cotacoes/{moeda}", ct);
            sw.Stop();

            return new MetricaConexao(
                ModoCliente: "Typed Client (Injeção de Dependência Fortemente Tipada)",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: 200,
                TentativasRealizadas: 1,
                Mensagem: "Requisição executada com sucesso através de Typed Client encapsulado.",
                Dados: cotacao
            );
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            var statusCode = (int?)ex.StatusCode ?? 500;
            return new MetricaConexao(
                ModoCliente: "Typed Client",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: statusCode,
                TentativasRealizadas: 1,
                Mensagem: $"Falha HTTP ao consultar cotação: {ex.Message}",
                Dados: null
            );
        }
    }
}
