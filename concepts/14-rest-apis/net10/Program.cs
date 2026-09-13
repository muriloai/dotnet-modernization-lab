using RestApisDemo.Endpoints;
using RestApisDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Registro de serviços para controllers convencionais
builder.Services.AddControllers();

// OpenAPI nativo integrado no .NET 10 (gera especificação sem necessidade de pacotes legados)
builder.Services.AddOpenApi();

// Injeção de dependência nativa do catálogo de produtos (Singleton para persistir durante a execução)
builder.Services.AddSingleton<IProdutoService, ProdutoService>();

var app = builder.Build();

// Servir arquivos estáticos para o painel interativo de testes (wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint OpenAPI em /openapi/v1.json
app.MapOpenApi();

// Mapeamento dos Controllers RESTful (/api/produtos)
app.MapControllers();

// Mapeamento dos Endpoints Minimal APIs (/api/v2/produtos)
app.MapProdutosEndpoints();

app.Run();
