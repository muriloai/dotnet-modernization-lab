using JsonSerializationDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SerializacaoBenchmarkService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
