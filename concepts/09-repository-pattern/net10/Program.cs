using Microsoft.EntityFrameworkCore;
using RepositoryPatternDemo.Data;
using RepositoryPatternDemo.Repositories;
using RepositoryPatternDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=pedidos.db";

// O DbContext é registrado como Scoped, atuando nativamente como Unit of Work por requisição
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Abordagem 1: Serviço consumindo o DbContext diretamente com projeções limpas DTO
builder.Services.AddScoped<PedidoService>();

// Abordagem 2: Repositório de domínio focado em regras e transações atômicas
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

var app = builder.Build();

// Inicialização da base SQLite
await DbInitializer.InicializarAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pedidos}/{action=Index}/{id?}");

app.Run();
