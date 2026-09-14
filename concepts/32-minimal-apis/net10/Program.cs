using MinimalApisDemo.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Mapeamento do Route Group com Minimal APIs
app.MapGroup("/api/tarefas").MapTarefasEndpoints();

app.Run();
