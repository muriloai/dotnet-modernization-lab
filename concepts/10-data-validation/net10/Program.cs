using DataValidationDemo.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro de Controllers com suporte a API
builder.Services.AddControllers();

// 2. Serviço global de ProblemDetails (RFC 7807 / RFC 9457)
// Centraliza a geração de respostas de erro padronizadas em toda a aplicação
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Adiciona informações complementares padronizadas ao payload de erro
        context.ProblemDetails.Extensions["ambiente"] = builder.Environment.EnvironmentName;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

var app = builder.Build();

// 3. Pipeline de middlewares do ASP.NET Core
// O UseExceptionHandler integrado com AddProblemDetails converte exceções não tratadas em ProblemDetails
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

// 4. Demonstração de Minimal API com TypedResults.ValidationProblem no .NET 10
app.MapPost("/api/minimal/validar-simples", (ClienteRequest request) =>
{
    var erros = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        erros["Nome"] = new[] { "O nome não pode estar vazio." };
    }

    if (erros.Count > 0)
    {
        // Gera resposta RFC 7807 fortemente tipada com status 400 Bad Request
        return Results.ValidationProblem(erros, title: "Falha de Validação na Minimal API");
    }

    return Results.Ok(new { Mensagem = "Dados validados com sucesso via Minimal API!", request.Nome });
});

app.Run();
