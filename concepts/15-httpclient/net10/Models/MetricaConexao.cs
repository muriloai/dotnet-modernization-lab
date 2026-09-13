namespace HttpClientDemo.Models;

/// <summary>
/// Modelo contendo métricas de diagnóstico da chamada HTTP externa.
/// Demonstra latência, código de resposta, número de tentativas (resiliência) e modo de consumo.
/// </summary>
public sealed record MetricaConexao(
    string ModoCliente,
    long TempoRespostaMs,
    int StatusCode,
    int TentativasRealizadas,
    string Mensagem,
    CotacaoMoeda? Dados
);
