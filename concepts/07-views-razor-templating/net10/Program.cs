using ViewsAndRazorDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Registro unificado de Controllers com suporte a Views e suporte a Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Injeção nativa de dependências
builder.Services.AddSingleton<ICatalogoService, CatalogoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Mapeamento de rotas convencionais de controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Produtos}/{action=Index}/{id?}");

// Mapeamento de endpoints de Razor Pages
app.MapRazorPages();

app.Run();
