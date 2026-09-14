using PedidosCqrsDemo.Features.Pedidos.Commands;
using PedidosCqrsDemo.Features.Pedidos.DTOs;
using PedidosCqrsDemo.Features.Pedidos.Queries;
using PedidosCqrsDemo.Infrastructure.Behaviors;
using PedidosCqrsDemo.Infrastructure.Mediator;
using PedidosCqrsDemo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Persistencia e Logs
builder.Services.AddSingleton<PedidoRepositoryEmMemoria>();
builder.Services.AddSingleton<TraceLogService>();

// Mediator
builder.Services.AddScoped<IMediator, MediatorDispatcher>();

// Handlers CQRS
builder.Services.AddScoped<IRequestHandler<CriarPedidoCommand, PedidoDetalheDto>, CriarPedidoHandler>();
builder.Services.AddScoped<IRequestHandler<ObterPedidosQuery, IReadOnlyList<PedidoDetalheDto>>, ObterPedidosHandler>();

// Pipeline Behaviors (registrados na ordem em que devem executar)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
