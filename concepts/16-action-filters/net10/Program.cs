using ActionFiltersDemo.Endpoints;
using ActionFiltersDemo.Filters;
using ActionFiltersDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. Registro do serviço de validação no container de DI
builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

// 2. Registro do filtro como serviço para permitir o uso de [ServiceFilter(typeof(ValidarApiKeyServiceFilter))]
builder.Services.AddScoped<ValidarApiKeyServiceFilter>();

// 3. Registro do filtro de medição de tempo
builder.Services.AddTransient<TempoExecucaoActionFilter>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapRelatoriosEndpoints();

app.Run();
