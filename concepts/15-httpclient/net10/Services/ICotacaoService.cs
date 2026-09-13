using HttpClientDemo.Models;

namespace HttpClientDemo.Services;

/// <summary>
/// Contrato do cliente tipado (Typed Client) para consumo de cotações.
/// A instância de HttpClient é gerenciada e injetada pelo IHttpClientFactory.
/// </summary>
public interface ICotacaoService
{
    Task<MetricaConexao> ObterCotacaoTypedClientAsync(string moeda, CancellationToken ct = default);
}
