namespace HttpClientDemo.Models;

/// <summary>
/// Modelo de dados representando a cotação de uma moeda estrangeira.
/// </summary>
public sealed record CotacaoMoeda(
    string Moeda,
    string Nome,
    decimal ValorReais,
    decimal VariacaoPercentual,
    DateTime DataHoraUtc,
    string Fonte
);
