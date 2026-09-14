using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryDemo.Models;
using OpenTelemetryDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TelemetryStore>();
builder.Services.AddSingleton<OrderProcessingService>();

// Configuracao padrao do ecossistema OpenTelemetry no .NET 10
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(TelemetryStore.ServiceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(TelemetryStore.ServiceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint: Criacao de pedido com rastreamento distribuido de spans
app.MapPost("/api/orders/checkout", async (OrderRequest? request, OrderProcessingService orderService, CancellationToken ct) =>
{
    var orderReq = request ?? new OrderRequest("TechCorp Brasil", "Licenca Enterprise Cloud", 4500.00m, "Cartao Corporativo");
    var result = await orderService.ProcessOrderAsync(orderReq, ct);
    return Results.Ok(result);
});

// Endpoint: Historico de traces recentes com spans encadeados
app.MapGet("/api/telemetry/traces", (TelemetryStore store) =>
{
    return Results.Ok(store.GetRecentTraces());
});

// Endpoint: Metricas em tempo real
app.MapGet("/api/telemetry/metrics", (TelemetryStore store) =>
{
    return Results.Ok(store.GetSnapshot());
});

app.Run();
