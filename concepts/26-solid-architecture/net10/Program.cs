using SolidArchitectureDemo.Application.Interfaces;
using SolidArchitectureDemo.Application.UseCases;
using SolidArchitectureDemo.Infrastructure.Repositories;
using SolidArchitectureDemo.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Camada de Infraestrutura
builder.Services.AddSingleton<IClienteRepository, ClienteRepositoryEmMemoria>();
builder.Services.AddScoped<INotificadorService, NotificadorConsoleService>();

// Camada de Aplicação (Use Cases)
builder.Services.AddScoped<CadastrarClienteUseCase>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
