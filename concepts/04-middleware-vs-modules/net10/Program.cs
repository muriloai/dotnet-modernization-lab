using MiddlewareVsModulesDemo.Middleware;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticFiles();

// -------------------------------------------------------------------------------------
// 1. Encadeamento Linear de Middlewares
// -------------------------------------------------------------------------------------

// Middleware 1: Medição de tempo de resposta (inicia antes de 'next' e conclui na volta)
app.UseMiddleware<RequestTimingMiddleware>();

// Middleware 2: Injeção de cabeçalhos de identificação da plataforma
app.UseMiddleware<CustomHeaderMiddleware>();

// Middleware 3: Middleware inline demonstrando pré e pós-processamento com delegate
app.Use(async (context, next) =>
{
    // Ação prévia antes de chamar o restante da cadeia
    context.Items["MiddlewareInlineExecutado"] = true;

    await next();

    // Ação executada na volta da requisição
    // No .NET 10, o código após 'await next()' é executado de forma garantida após os middlewares filhos
});

// -------------------------------------------------------------------------------------
// 2. Ramificação de Pipeline com app.Map (Substituto moderno do IHttpHandler .ashx)
// -------------------------------------------------------------------------------------
// Isola completamente a rota /health do pipeline downstream.
// Middlewares registrados após o 'Map' não são executados para esta rota.
app.Map("/health", healthApp =>
{
    healthApp.Run(async context =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            Status = "Saudável",
            Descricao = "Endpoint terminal isolado via app.Map() (equivalente moderno ao IHttpHandler).",
            Servidor = "Kestrel",
            Timestamp = DateTime.UtcNow
        });
    });
});

// -------------------------------------------------------------------------------------
// 3. Pipeline Condicional com app.UseWhen
// -------------------------------------------------------------------------------------
// Aplica um middleware extra apenas quando a requisição satisfaz uma condição (rotas que começam com /api)
app.UseWhen(
    predicate: context => context.Request.Path.StartsWithSegments("/api"),
    configuration: apiApp =>
    {
        apiApp.Use(async (context, next) =>
        {
            context.Response.Headers["X-Zone"] = "Area-de-APIs";
            await next();
        });
    });

// -------------------------------------------------------------------------------------
// 4. Endpoints da Aplicação
// -------------------------------------------------------------------------------------

// Endpoint de API para testar o pipeline condicional
app.MapGet("/api/ping", () => Results.Ok(new
{
    Mensagem = "pong",
    Pipeline = "Executado através dos middlewares globais e do ramo condicional /api.",
    Horario = DateTime.UtcNow
}));

// Rota Raiz: Painel visual explicativo em HTML puro
app.MapGet("/", (HttpContext context) =>
{
    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Lab 04: Middlewares vs Módulos HTTP (.NET 10)</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 04: Middlewares vs Módulos HTTP</h1>
            <p class="subtitle">Demonstração de encadeamento linear, fluxo bidirecional, app.Map e app.UseWhen.</p>

            <div class="grid">
                <div class="card">
                    <h2>Sequência do Pipeline Moderno</h2>
                    <div class="pipeline-step">
                        <strong>1. RequestTimingMiddleware</strong><br>
                        Inicia Stopwatch e registra evento OnStarting.
                    </div>
                    <div class="pipeline-step">
                        <strong>2. CustomHeaderMiddleware</strong><br>
                        Injeta os cabeçalhos <code>X-Execution-Engine</code> e <code>X-Pipeline-Type</code>.
                    </div>
                    <div class="pipeline-step">
                        <strong>3. Middleware Inline (app.Use)</strong><br>
                        Armazena itens no <code>HttpContext.Items</code> e aguarda a volta.
                    </div>
                    <div class="pipeline-step">
                        <strong>4. Endpoint Final / Minimal API</strong><br>
                        Gera o resultado HTML ou JSON da requisição.
                    </div>
                </div>

                <div class="card">
                    <h2>Cabeçalhos Injetados pelos Middlewares</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">
                        Estes valores são adicionados no fluxo de retorno e podem ser inspecionados nas Ferramentas de Desenvolvedor (F12):
                    </p>
                    <ul class="feature-list">
                        <li><span>X-Execution-Engine:</span> <strong class="tag">DotNet-10-Kestrel</strong></li>
                        <li><span>X-Pipeline-Type:</span> <strong class="tag">ASP.NET-Core-Middleware</strong></li>
                        <li><span>X-Response-Time-Ms:</span> <strong class="tag">Calculado dinamicamente</strong></li>
                    </ul>
                    <h3 style="margin-top: 1rem;">Equivalente ao IHttpHandler</h3>
                    <p style="font-size: 0.85rem; color: #495057;">
                        A rota <code>/health</code> foi isolada com <code>app.Map()</code>, dispensando a criação de arquivos <code>.ashx</code>.
                    </p>
                </div>
            </div>

            <div class="note-box">
                <strong>Como inspecionar o fluxo bidirecional:</strong>
                Abra as <strong>Ferramentas de Desenvolvedor do Navegador (F12)</strong>, clique na aba <strong>Rede (Network)</strong> e recarregue a página (F5).
                Selecione o documento raiz e visualize a seção <strong>Cabeçalhos de Resposta (Response Headers)</strong> para verificar os dados injetados pelos middlewares.
            </div>

            <div class="endpoints-box">
                <h3>Endpoints Disponíveis para Teste</h3>
                <a class="endpoint-link" href="/health" target="_blank">GET /health (Endpoint terminal isolado com app.Map)</a>
                <a class="endpoint-link" href="/api/ping" target="_blank">GET /api/ping (Dispara o pipeline condicional app.UseWhen)</a>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
