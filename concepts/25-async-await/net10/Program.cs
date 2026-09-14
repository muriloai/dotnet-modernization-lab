using AsyncAwaitDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<OperacoesAssincronasModernoService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
