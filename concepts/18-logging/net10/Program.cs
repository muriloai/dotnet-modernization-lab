using LoggingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuracao do provedor de logs em memoria
var provedorMemoria = new ProvedorLogsMemoria();
builder.Services.AddSingleton<IProvedorLogsMemoria>(provedorMemoria);
builder.Logging.AddProvider(provedorMemoria);

// Servicos de aplicacao
builder.Services.AddScoped<ProcessadorPedidosService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
