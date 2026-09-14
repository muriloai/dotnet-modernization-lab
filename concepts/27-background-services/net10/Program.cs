using BackgroundServicesDemo.Services;
using BackgroundServicesDemo.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IFilaProcessamento, FilaProcessamentoChannel>();
builder.Services.AddSingleton<EstadoMonitoramento>();
builder.Services.AddHostedService<FilaProcessamentoWorker>();
builder.Services.AddHostedService<MonitoramentoPeriodicoWorker>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
