namespace OpenTelemetryDemo.Models;

public record OrderRequest(
    string Cliente,
    string Produto,
    decimal Valor,
    string MetodoPagamento
);

public record SpanDetailDto(
    string Name,
    string SpanId,
    string? ParentSpanId,
    string TraceId,
    long DurationMs,
    string Status,
    Dictionary<string, string> Tags,
    List<string> Events
);

public record TraceSummaryDto(
    string TraceId,
    string RootName,
    long TotalDurationMs,
    DateTime Timestamp,
    List<SpanDetailDto> Spans
);

public record OrderCheckoutResult(
    string OrderId,
    string TraceId,
    string W3CTraceParent,
    long DurationMs,
    string Status,
    string Mensagem,
    List<SpanDetailDto> Spans
);

public record MetricSnapshotDto(
    long TotalPedidos,
    double ValorTotalProcessado,
    long UltimaDuracaoMs,
    string StatusColetores
);
