using System;
using CachingDemo.Policies;
using CachingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Cache em Memória
builder.Services.AddMemoryCache();

// Cache Distribuído (em memória para laboratório, substituível por Redis/SQL Server sem alterar código)
builder.Services.AddDistributedMemoryCache();

// Serviço de Negócio do Catálogo
builder.Services.AddSingleton<ICatalogoService, CatalogoService>();

// Output Caching com Política Personalizada e Invalidação por Tags
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PorCategoria", CategoriaOutputCachePolicy.Instance);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// Middleware de Output Caching
app.UseOutputCache();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
