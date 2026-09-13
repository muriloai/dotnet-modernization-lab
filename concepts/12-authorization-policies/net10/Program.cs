using AuthorizationPoliciesDemo.Authorization.Handlers;
using AuthorizationPoliciesDemo.Authorization.Requirements;
using AuthorizationPoliciesDemo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços e Repositórios
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IRelatorioService, RelatorioService>();

// 2. Registro dos Handlers de Autorização no Contêiner de DI
// No .NET 10, qualquer classe AuthorizationHandler é gerenciada pelo contêiner de DI com ciclo de vida
builder.Services.AddSingleton<IAuthorizationHandler, IdadeMinimaHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ExperienciaMinimaHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DonoDoRelatorioHandler>();

// 3. Autenticação por Cookie com respostas REST para 401 e 403
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ModernLab.AuthPolicies";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// 4. Configuração das Políticas de Autorização (Policy-Based Authorization)
builder.Services.AddAuthorization(options =>
{
    // Política 1: Baseada em Claim simples de departamento
    options.AddPolicy("ApenasFinanceiro", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("departamento", "Financeiro"));

    // Política 2: Requisito Customizado desacoplado (Idade mínima de 18 anos)
    options.AddPolicy("MaioridadeLegal", policy =>
        policy.RequireAuthenticatedUser()
              .Requirements.Add(new IdadeMinimaRequirement(18)));

    // Política 3: Composta (Papéis + Claim de Departamento + Requisito de Experiência >= 5 anos)
    options.AddPolicy("AprovadorSenior", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Administrador", "Gerente")
              .RequireClaim("departamento", "Financeiro")
              .Requirements.Add(new ExperienciaMinimaRequirement(5)));

    // Política 4: Autorização Baseada em Recurso (Dono da entidade ou Administrador)
    options.AddPolicy("DonoOuAdministrador", policy =>
        policy.RequireAuthenticatedUser()
              .Requirements.Add(new DonoDoRelatorioRequirement()));
});

var app = builder.Build();

// 5. Pipeline HTTP
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
