using SignalRDemo.Hubs;
using SignalRDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registro dos serviços de SignalR no ASP.NET Core (.NET 10)
builder.Services.AddSignalR();

// Registro do serviço de broadcast desacoplado utilizando IHubContext
builder.Services.AddSingleton<ITransmissorEventos, TransmissorEventos>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Mapeamento do endpoint do Hub fortemente tipado no pipeline Kestrel
app.MapHub<NotificacoesHub>("/hubs/notificacoes");

app.Run();
