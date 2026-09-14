using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetryDemo.Models;

namespace OpenTelemetryDemo.Services;

public class TelemetryStore
{
    private readonly ConcurrentQueue<TraceSummaryDto> _traces = new();
    private long _totalOrders = 0;
    private double _totalAmount = 0;
    private long _lastDurationMs = 0;

    public static readonly string ServiceName = "ModernizationLab.OrderService";
    public static readonly ActivitySource ActivitySource = new(ServiceName, "1.0.0");
    public static readonly Meter Meter = new(ServiceName, "1.0.0");

    // Instrumentos do System.Diagnostics.Metrics
    private static readonly Counter<long> OrdersCounter = Meter.CreateCounter<long>(
        "orders.completed.count",
        description: "Contagem total de pedidos finalizados com sucesso");

    private static readonly Histogram<long> OrderProcessingDuration = Meter.CreateHistogram<long>(
        "orders.processing.duration",
        unit: "ms",
        description: "Distribuicao de tempo de processamento dos pedidos");

    public void AddTrace(TraceSummaryDto trace)
    {
        _traces.Enqueue(trace);
        while (_traces.Count > 50)
        {
            _traces.TryDequeue(out _);
        }
    }

    public List<TraceSummaryDto> GetRecentTraces()
    {
        return _traces.Reverse().Take(10).ToList();
    }

    public void RecordOrderMetrics(decimal valor, long durationMs)
    {
        Interlocked.Increment(ref _totalOrders);
        OrdersCounter.Add(1);
        OrderProcessingDuration.Record(durationMs);
        Interlocked.Exchange(ref _lastDurationMs, durationMs);
    }

    public MetricSnapshotDto GetSnapshot()
    {
        return new MetricSnapshotDto(
            Interlocked.Read(ref _totalOrders),
            _totalAmount,
            Interlocked.Read(ref _lastDurationMs),
            "OpenTelemetry OTLP Exporter ativo - System.Diagnostics.Metrics integrado"
        );
    }
}

public class OrderProcessingService
{
    private readonly TelemetryStore _telemetryStore;

    public OrderProcessingService(TelemetryStore telemetryStore)
    {
        _telemetryStore = telemetryStore;
    }

    public async Task<OrderCheckoutResult> ProcessOrderAsync(OrderRequest request, CancellationToken ct = default)
    {
        var spansList = new List<SpanDetailDto>();
        var orderId = "ORD-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var swTotal = Stopwatch.StartNew();

        // 1. Root Activity: CheckoutOrder
        using (var rootActivity = TelemetryStore.ActivitySource.StartActivity("CheckoutOrder", ActivityKind.Server))
        {
            var traceId = rootActivity?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
            var rootSpanId = rootActivity?.SpanId.ToString() ?? Guid.NewGuid().ToString("N")[..16];
            var traceParent = rootActivity?.Id ?? $"00-{traceId}-{rootSpanId}-01";

            rootActivity?.SetTag("order.id", orderId);
            rootActivity?.SetTag("customer.name", request.Cliente);
            rootActivity?.SetTag("payment.method", request.MetodoPagamento);
            rootActivity?.SetTag("order.amount", request.Valor.ToString("F2"));

            // 2. Child Activity 1: ValidatePayment
            var swPayment = Stopwatch.StartNew();
            using (var paymentActivity = TelemetryStore.ActivitySource.StartActivity("ValidatePayment", ActivityKind.Internal, rootActivity?.Context ?? default))
            {
                paymentActivity?.SetTag("payment.provider", "GatewayStone");
                paymentActivity?.SetTag("payment.card_type", "Credit");
                await Task.Delay(120, ct); // Simula Gateway externo

                paymentActivity?.AddEvent(new ActivityEvent("PaymentAuthorized", DateTimeOffset.UtcNow));
                swPayment.Stop();

                spansList.Add(new SpanDetailDto(
                    "ValidatePayment",
                    paymentActivity?.SpanId.ToString() ?? "sub-1",
                    rootSpanId,
                    traceId,
                    swPayment.ElapsedMilliseconds,
                    "Ok",
                    new Dictionary<string, string>
                    {
                        ["payment.provider"] = "GatewayStone",
                        ["payment.status"] = "Authorized",
                        ["payment.value"] = request.Valor.ToString("C")
                    },
                    new List<string> { "PaymentAuthorized" }
                ));
            }

            // 3. Child Activity 2: UpdateInventory
            var swInventory = Stopwatch.StartNew();
            using (var invActivity = TelemetryStore.ActivitySource.StartActivity("UpdateInventory", ActivityKind.Internal, rootActivity?.Context ?? default))
            {
                invActivity?.SetTag("db.system", "postgresql");
                invActivity?.SetTag("inventory.item", request.Produto);
                invActivity?.SetTag("inventory.reserved_qty", "1");
                await Task.Delay(80, ct); // Simula baixa no banco

                invActivity?.AddEvent(new ActivityEvent("StockReserved", DateTimeOffset.UtcNow));
                swInventory.Stop();

                spansList.Add(new SpanDetailDto(
                    "UpdateInventory",
                    invActivity?.SpanId.ToString() ?? "sub-2",
                    rootSpanId,
                    traceId,
                    swInventory.ElapsedMilliseconds,
                    "Ok",
                    new Dictionary<string, string>
                    {
                        ["db.system"] = "postgresql",
                        ["item.name"] = request.Produto,
                        ["inventory.status"] = "Reserved"
                    },
                    new List<string> { "StockReserved" }
                ));
            }

            // 4. Child Activity 3: DispatchNotification
            var swNotify = Stopwatch.StartNew();
            using (var notifyActivity = TelemetryStore.ActivitySource.StartActivity("DispatchNotification", ActivityKind.Producer, rootActivity?.Context ?? default))
            {
                notifyActivity?.SetTag("messaging.destination", "notifications.orders");
                notifyActivity?.SetTag("messaging.system", "rabbitmq");
                await Task.Delay(50, ct); // Simula envio de mensagem

                swNotify.Stop();

                spansList.Add(new SpanDetailDto(
                    "DispatchNotification",
                    notifyActivity?.SpanId.ToString() ?? "sub-3",
                    rootSpanId,
                    traceId,
                    swNotify.ElapsedMilliseconds,
                    "Ok",
                    new Dictionary<string, string>
                    {
                        ["messaging.system"] = "rabbitmq",
                        ["queue"] = "notifications.orders"
                    },
                    new List<string> { "MessagePublished" }
                ));
            }

            swTotal.Stop();

            // Adiciona Root Span aos detalhes
            var rootSpan = new SpanDetailDto(
                "CheckoutOrder",
                rootSpanId,
                null,
                traceId,
                swTotal.ElapsedMilliseconds,
                "Ok",
                new Dictionary<string, string>
                {
                    ["order.id"] = orderId,
                    ["customer.name"] = request.Cliente,
                    ["order.total"] = request.Valor.ToString("C")
                },
                new List<string> { "OrderCompleted" }
            );

            // Grava nas métricas do OpenTelemetry
            _telemetryStore.RecordOrderMetrics(request.Valor, swTotal.ElapsedMilliseconds);

            var traceSummary = new TraceSummaryDto(
                traceId,
                "CheckoutOrder",
                swTotal.ElapsedMilliseconds,
                DateTime.UtcNow,
                new List<SpanDetailDto> { rootSpan, spansList[0], spansList[1], spansList[2] }
            );
            _telemetryStore.AddTrace(traceSummary);

            return new OrderCheckoutResult(
                orderId,
                traceId,
                traceParent,
                swTotal.ElapsedMilliseconds,
                "Aprovado",
                "Pedido concluido e rastreado com sucesso via ActivitySource e OpenTelemetry.",
                new List<SpanDetailDto> { rootSpan, spansList[0], spansList[1], spansList[2] }
            );
        }
    }
}
