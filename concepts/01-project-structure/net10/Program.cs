// =============================================================================
// Lab 01: Estrutura de Projeto e Inicialização no .NET 10
// =============================================================================
// No .NET 10 (C# 14), todo o ciclo de vida da aplicação é configurado aqui em
// Program.cs através de Top-Level Statements.
//
// Diferenças fundamentais em relação ao .NET Framework 4.8.1:
// 1. Sem classe Program estática, sem método 'static void Main(string[] args)'.
// 2. Sem arquivo Global.asax.cs nem eventos de HttpApplication.
// 3. Servidor web Kestrel embutido, dispensando o IIS para execução.
// 4. Injeção de dependências (builder.Services) e pipeline HTTP (app.Use...)
//    configurados no mesmo arquivo de forma unificada e fluida.
// =============================================================================

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços essenciais ao container nativo de injeção de dependências
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

// Endpoint raiz: Retorna uma página HTML didática explicando a arquitetura .NET 10
app.MapGet("/", (IConfiguration config, IWebHostEnvironment env) =>
{
    var appName = config["ApplicationInfo:Name"] ?? ".NET 10 App";
    var appVersion = config["ApplicationInfo:Version"] ?? "10.0";
    var currentEnv = env.EnvironmentName;

    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>{{appName}}</title>
        <style>
            :root {
                --bg: #f8fafc;
                --card-bg: #ffffff;
                --text: #0f172a;
                --text-muted: #475569;
                --primary: #2563eb;
                --border: #e2e8f0;
                --code-bg: #f1f5f9;
                --success: #16a34a;
            }
            body {
                font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                background-color: var(--bg);
                color: var(--text);
                margin: 0;
                padding: 2rem;
                line-height: 1.6;
            }
            .container {
                max-width: 800px;
                margin: 0 auto;
            }
            .badge {
                display: inline-block;
                padding: 0.25rem 0.75rem;
                border-radius: 9999px;
                font-size: 0.875rem;
                font-weight: 600;
                background-color: #dbeafe;
                color: var(--primary);
                margin-bottom: 1rem;
            }
            .card {
                background-color: var(--card-bg);
                border: 1px solid var(--border);
                border-radius: 0.5rem;
                padding: 1.5rem;
                margin-bottom: 1.5rem;
                box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
            }
            h1, h2, h3 { margin-top: 0; color: var(--text); }
            pre {
                background-color: var(--code-bg);
                padding: 1rem;
                border-radius: 0.375rem;
                overflow-x: auto;
                font-family: Consolas, Monaco, monospace;
                font-size: 0.9rem;
            }
            .feature-list { list-style-type: none; padding-left: 0; }
            .feature-list li {
                padding: 0.5rem 0;
                border-bottom: 1px solid var(--border);
            }
            .feature-list li:last-child { border-bottom: none; }
            .status-indicator {
                color: var(--success);
                font-weight: bold;
            }
        </style>
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 01: Estrutura de Projeto e Bootstrap</h1>
            <p>Este serviço está sendo executado nativamente através do servidor embutido <strong>Kestrel</strong>, sem dependência do IIS.</p>

            <div class="card">
                <h2>Informações do Runtime</h2>
                <ul class="feature-list">
                    <li><strong>Aplicação:</strong> {{appName}}</li>
                    <li><strong>Versão:</strong> {{appVersion}}</li>
                    <li><strong>Ambiente Atual:</strong> {{currentEnv}}</li>
                    <li><strong>Status do Servidor:</strong> <span class="status-indicator">Em Execução (Kestrel)</span></li>
                    <li><strong>Framework Alvo:</strong> .NET 10.0 (C# 14)</li>
                </ul>
            </div>

            <div class="card">
                <h2>Como Funciona a Inicialização (Program.cs)</h2>
                <p>Veja como é direto: a configuração inteira fica em poucas linhas.</p>
                <pre><code>var builder = WebApplication.CreateBuilder(args);
                // 1. Configuração de serviços de injeção de dependência
                builder.Services.AddEndpointsApiExplorer();

                var app = builder.Build();

                // 2. Pipeline de Middlewares
                app.MapGet("/", () => Results.Ok("Olá Mundo"));

                // 3. Execução autohospedada
                app.Run();</code></pre>
            </div>

            <div class="card">
                <h2>Endpoints Disponíveis</h2>
                <ul class="feature-list">
                    <li><a href="/api/info">/api/info</a>: Retorna informações técnicas em formato JSON.</li>
                    <li><a href="/api/comparison">/api/comparison</a>: Comparativo direto com o .NET Framework.</li>
                </ul>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

// Endpoint de metadados em JSON
app.MapGet("/api/info", (IConfiguration config, IWebHostEnvironment env) =>
{
    return Results.Ok(new
    {
        framework = ".NET 10",
        csharpVersion = "C# 14",
        hostingModel = "Kestrel (Self-hosted In-Process)",
        projectFileFormat = "SDK-style minimal XML",
        entryPoint = "Program.cs (Top-Level Statements)",
        configurationSource = "appsettings.json + IConfiguration",
        environment = env.EnvironmentName,
        timestamp = DateTimeOffset.UtcNow
    });
});

// Endpoint comparativo didático
app.MapGet("/api/comparison", () =>
{
    return Results.Ok(new
    {
        conceito = "Lab 01 - Estrutura de Projeto e Inicialização",
        netFramework481 = new
        {
            arquivoProjeto = "ProjectStructure.csproj (XML verboso com mais de 100 linhas e GUIDs)",
            pontoDeEntrada = "Global.asax.cs -> Application_Start()",
            servidorObrigatorio = "Windows IIS / IIS Express (System.Web.dll)",
            gerenciadorPacotes = "packages.config + bindingRedirect em Web.config",
            configuracoes = "Web.config em XML complexo"
        },
        net10 = new
        {
            arquivoProjeto = "ProjectStructure.csproj (SDK-style limpo com menos de 10 linhas)",
            pontoDeEntrada = "Program.cs com Top-Level Statements",
            servidorObrigatorio = "Kestrel nativo multiplataforma (Linux, macOS, Windows)",
            gerenciadorPacotes = "PackageReference transitivo sem bindingRedirects",
            configuracoes = "appsettings.json hierárquico e tipado"
        }
    });
});

app.Run();
