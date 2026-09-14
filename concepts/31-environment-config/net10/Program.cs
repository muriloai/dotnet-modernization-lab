using EnvironmentDemo.Services;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Microsoft Feature Management
builder.Services.AddFeatureManagement();
builder.Services.AddSingleton<GerenciadorFlagsEmMemoria>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
