using HttpClientDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Simulador de falhas transientes para testes de resiliência
builder.Services.AddSingleton<ISimuladorFalhasService, SimuladorFalhasService>();

// 1. Registro do IHttpClientFactory para uso de Basic Clients
builder.Services.AddHttpClient();

// 2. Registro de Named Client configurado centralmente
builder.Services.AddHttpClient("CotacoesNomeado", client =>
{
    client.BaseAddress = new Uri("http://localhost:6500/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "DotNet10-NamedClient/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    // Reciclagem de conexões para revalidar DNS automaticamente
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
});

// 3. Registro de Typed Client (injetado via interface ICotacaoService)
builder.Services.AddHttpClient<ICotacaoService, CotacaoService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:6500/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "DotNet10-TypedClient/1.0");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(2));

// 4. Registro de Resilient Client com pipeline de resiliência padrão (.NET 10 / Polly)
builder.Services.AddHttpClient("CotacoesResiliente", client =>
{
    client.BaseAddress = new Uri("http://localhost:6500/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "DotNet10-ResilientClient/1.0");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(150);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
