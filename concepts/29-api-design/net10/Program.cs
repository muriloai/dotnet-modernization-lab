using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuracao de Versionamento de API
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddMvc();

// Padronizacao RFC 7807 ProblemDetails
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
