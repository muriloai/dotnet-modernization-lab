using HealthChecksDemo.Checks;
using HealthChecksDemo.Formatters;
using HealthChecksDemo.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Registro do simulador de estado dos recursos
builder.Services.AddSingleton<ISimuladorEstadoRecursos, SimuladorEstadoRecursos>();

// Configuracao nativa de Health Checks com tags para liveness e readiness
builder.Services.AddHealthChecks()
    .AddCheck<MemoriaHealthCheck>("memoria", tags: ["live", "ready"])
    .AddCheck<BancoDadosHealthCheck>("banco_dados", tags: ["ready"])
    .AddCheck<ServicoMensageriaHealthCheck>("mensageria", tags: ["ready"]);

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint de Liveness: avalia apenas se o processo esta vivo (tag: live)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriters.EscreverRespostaJson
});

// Endpoint de Readiness: avalia se todas as dependencias estao prontas para trafego (tag: ready)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriters.EscreverRespostaJson
});

// Endpoint Geral: executa todas as verificacoes cadastradas
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriters.EscreverRespostaJson
});

app.MapControllers();

app.Run();
