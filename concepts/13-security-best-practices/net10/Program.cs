using SecurityBestPracticesDemo.Middlewares;
using SecurityBestPracticesDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços e Injeção de Dependências
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ITransferenciaService, TransferenciaService>();

// 2. Configuração de Anti-CSRF (Antiforgery)
// No .NET 10, o Antiforgery opera de forma unificada com cabeçalhos de requisição customizados para APIs e AJAX
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = ".ModernLab.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// 3. Configuração de Políticas Restritivas de CORS
// Substitui a liberação genérica '*' do Web.config por controles em nível de rota e domínio
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaRestritaParceiro", policy =>
    {
        policy.WithOrigins("https://portal-parceiro.empresa.com.br")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization", "X-XSRF-TOKEN");
    });
});

var app = builder.Build();

// 4. Middlewares de Segurança no Pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

// Injeção de cabeçalhos de proteção (X-Frame-Options, CSP, nosniff, etc.)
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// CORS deve ser executado após o roteamento para associar as políticas dos endpoints
app.UseCors();

// Antiforgery middleware ativado no pipeline HTTP
app.UseAntiforgery();

app.MapControllers();

app.Run();
