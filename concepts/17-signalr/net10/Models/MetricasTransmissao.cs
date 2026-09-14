namespace SignalRDemo.Models;

/// <summary>
/// Métricas do Hub distribuídas em tempo real para os clientes conectados.
/// </summary>
public sealed record MetricasTransmissao(
    int ConexoesAtivas,
    int TotalMensagensTrafegadas,
    DateTime UltimoHeartbeat
);
