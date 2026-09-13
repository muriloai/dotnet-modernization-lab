using ControllersAndActionsDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a Controllers no container de injeção de dependências
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();

// Mapeia todos os controllers anotados com [Route] e [ApiController]
app.MapControllers();

// -------------------------------------------------------------------------------------
// Demonstração de Minimal API equivalente com TypedResults
// No .NET moderno, controllers e Minimal APIs compartilham os mesmos conceitos de resultado.
// -------------------------------------------------------------------------------------
app.MapGet("/api/minimal/produtos", () =>
{
    var lista = new List<ProdutoDto>
    {
        new(99, "Mouse Sem Fio (Via Minimal API)", "perifericos", 180.00m, true)
    };
    return TypedResults.Ok(lista);
});

// -------------------------------------------------------------------------------------
// Rota Raiz: Painel visual explicativo em HTML puro
// -------------------------------------------------------------------------------------
app.MapGet("/", () =>
{
    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Lab 06: Controllers e Actions (.NET 10)</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 06: Controllers e Actions</h1>
            <p class="subtitle">Demonstração de ControllerBase unificado, atributo [ApiController] e validação automática com ProblemDetails.</p>

            <div class="grid">
                <div class="card">
                    <h2>Recursos do [ApiController]</h2>
                    <ul class="feature-list">
                        <li><span>Inferência de Binding:</span> <span class="tag">[FromBody], [FromRoute]</span></li>
                        <li><span>Validação Automática:</span> <span class="tag">Ativa no Pipeline</span></li>
                        <li><span>Formato de Erro:</span> <span class="tag">RFC 7807 ProblemDetails</span></li>
                        <li><span>Classe Base:</span> <span class="tag">ControllerBase</span></li>
                    </ul>
                </div>

                <div class="card">
                    <h2>Boilerplate Eliminado</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">
                        No .NET 10, você <strong>não precisa</strong> escrever:
                    <pre style="background: #282c34; color: #e06c75; padding: 0.75rem; border-radius: 4px; font-size: 0.85rem; font-family: monospace;">
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                    </pre>
                    <p style="margin-top: 0.5rem; font-size: 0.85rem; color: #495057;">
                        O framework valida as anotações do request e rejeita requisições inválidas automaticamente com código 400.
                    </p>
                </div>
            </div>

            <div class="card" style="margin-bottom: 1.5rem;">
                <h2>Como Testar a Validação Automática via Terminal</h2>
                <p style="font-size: 0.9rem; color: #495057; margin-bottom: 0.5rem;">
                    Execute o comando abaixo com payload inválido (nome vazio e preço negativo) para observar o <strong>ProblemDetails</strong>:
                <pre style="background: #282c34; color: #98c379; padding: 1rem; border-radius: 4px; font-size: 0.85rem; overflow-x: auto; font-family: monospace;">
                    curl -X POST http://localhost:5600/api/produtos \
                      -H "Content-Type: application/json" \
                      -d "{\"nome\":\"\",\"categoria\":\"hardware\",\"preco\":-20.00}"
                </pre>
            </div>

            <div class="endpoints-box">
                <h3>Endpoints Disponíveis</h3>
                <a class="endpoint-link" href="/api/produtos" target="_blank">GET /api/produtos (Listar via Controller ControllerBase)</a>
                <a class="endpoint-link" href="/api/produtos/1" target="_blank">GET /api/produtos/1 (Obter com ActionResult&lt;ProdutoDto&gt;)</a>
                <a class="endpoint-link" href="/api/minimal/produtos" target="_blank">GET /api/minimal/produtos (Demonstração com TypedResults)</a>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
