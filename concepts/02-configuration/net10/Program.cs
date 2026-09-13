using ConfigurationDemo.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------------------
// 1. Registro do Options Pattern com Validação Antecipada no Startup
// -------------------------------------------------------------------------------------
// No .NET 10, vinculamos as seções do appsettings.json a classes POCO tipadas.
// As chamadas ValidateDataAnnotations() e ValidateOnStart() garantem o princípio "fail-fast":
// se uma configuração obrigatória estiver ausente ou inválida, a aplicação nem sequer inicializa.
builder.Services
    .AddOptions<ConfiguracaoGeralOptions>()
    .Bind(builder.Configuration.GetSection(ConfiguracaoGeralOptions.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.UseStaticFiles();

// -------------------------------------------------------------------------------------
// 2. Endpoints Demonstrando os Diferentes Métodos de Leitura
// -------------------------------------------------------------------------------------

// Endpoint 1: Leitura direta através de IConfiguration (semelhante ao ConfigurationManager, mas injetável)
app.MapGet("/api/config/direta", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        Metodo = "IConfiguration direto",
        Descricao = "Lê valores em texto ou com conversão manual. Não utiliza POCO nem validação automática.",
        NomeSistema = config["ConfiguracaoGeral:NomeSistema"],
        Ambiente = config["ConfiguracaoGeral:Ambiente"],
        LimiteItensPorPagina = config.GetValue<int>("ConfiguracaoGeral:LimiteItensPorPagina"),
        StringConexao = config.GetConnectionString("DefaultConnection")
    });
});

// Endpoint 2: IOptions<T> (Ciclo de Vida: Singleton)
// Lê os dados apenas uma vez na subida. Possui altíssima performance, mas não recarrega se o JSON mudar.
app.MapGet("/api/config/options", (IOptions<ConfiguracaoGeralOptions> options) =>
{
    return Results.Ok(new
    {
        Metodo = "IOptions<T>",
        CicloDeVida = "Singleton",
        ComportamentoHotReload = "Estático (valores fixados na inicialização da aplicação)",
        Valores = options.Value
    });
});

// Endpoint 3: IOptionsSnapshot<T> (Ciclo de Vida: Scoped)
// Recalcula os valores a cada requisição HTTP. Reflete alterações recentes preservando consistência na requisição.
app.MapGet("/api/config/snapshot", (IOptionsSnapshot<ConfiguracaoGeralOptions> snapshot) =>
{
    return Results.Ok(new
    {
        Metodo = "IOptionsSnapshot<T>",
        CicloDeVida = "Scoped (por requisição HTTP)",
        ComportamentoHotReload = "Recalculado a cada requisição",
        Valores = snapshot.Value
    });
});

// Endpoint 4: IOptionsMonitor<T> (Ciclo de Vida: Singleton Reativo)
// Permite leitura imediata da propriedade CurrentValue e dispara eventos OnChange quando o arquivo é editado.
app.MapGet("/api/config/monitor", (IOptionsMonitor<ConfiguracaoGeralOptions> monitor) =>
{
    return Results.Ok(new
    {
        Metodo = "IOptionsMonitor<T>",
        CicloDeVida = "Singleton com Recarregamento Dinâmico (Hot Reload)",
        ComportamentoHotReload = "Atualizado imediatamente em memória sem reiniciar o processo",
        TimestampLeitura = DateTime.Now.ToString("HH:mm:ss.fff"),
        Valores = monitor.CurrentValue
    });
});

// Endpoint 5: Configuração de SMTP tipada
app.MapGet("/api/config/smtp", (IOptions<SmtpOptions> smtpOptions) =>
{
    return Results.Ok(new
    {
        Metodo = "IOptions<SmtpOptions>",
        Descricao = "Classe POCO tipada com validação declarativa de e-mail e porta.",
        Valores = smtpOptions.Value
    });
});

// Rota Raiz: Painel visual explicativo em HTML puro
app.MapGet("/", (
    IOptions<ConfiguracaoGeralOptions> options,
    IOptionsMonitor<ConfiguracaoGeralOptions> monitor,
    IOptions<SmtpOptions> smtpOptions,
    IConfiguration config) =>
{
    var connString = config.GetConnectionString("DefaultConnection") ?? "Não configurada";
    var geral = options.Value;
    var geralAtual = monitor.CurrentValue;
    var smtp = smtpOptions.Value;

    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Lab 02: Configuração e Options Pattern (.NET 10)</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 02: Configuração e Options Pattern</h1>
            <p class="subtitle">Demonstração de IConfiguration, IOptions, IOptionsSnapshot e IOptionsMonitor com validação precoce.</p>

            <div class="grid">
                <div class="card">
                    <h2>Configuração Geral (Options Pattern)</h2>
                    <ul class="feature-list">
                        <li><span>Sistema:</span> <strong class="tag">{{geralAtual.NomeSistema}}</strong></li>
                        <li><span>Ambiente:</span> <strong class="tag">{{geralAtual.Ambiente}}</strong></li>
                        <li><span>Itens por Página:</span> <strong class="tag">{{geralAtual.LimiteItensPorPagina}}</strong></li>
                        <li><span>Auditoria Ativa:</span> <strong class="tag">{{(geralAtual.HabilitarAuditoria ? "Sim" : "Não")}}</strong></li>
                    </ul>
                    <h3>String de Conexão</h3>
                    <p class="tag" style="word-break: break-all;">{{connString}}</p>
                </div>

                <div class="card">
                    <h2>Configuração SMTP Tipada (POCO)</h2>
                    <ul class="feature-list">
                        <li><span>Servidor:</span> <strong class="tag">{{smtp.Servidor}}</strong></li>
                        <li><span>Porta:</span> <strong class="tag">{{smtp.Porta}}</strong></li>
                        <li><span>SSL Habilitado:</span> <strong class="tag">{{(smtp.HabilitarSsl ? "Sim" : "Não")}}</strong></li>
                        <li><span>Remetente:</span> <strong class="tag">{{smtp.EmailRemetente}}</strong></li>
                    </ul>
                    <h3>Validação Aplicada</h3>
                    <p style="font-size: 0.85rem; color: #6c757d;">Validado via DataAnnotations (EmailAddress, Range e Required) durante a subida com ValidateOnStart().</p>
                </div>
            </div>

            <div class="card" style="margin-bottom: 1.5rem;">
                <h2>Comparativo de Ciclos de Vida das Opções</h2>
                <table style="width: 100%; border-collapse: collapse; font-size: 0.95rem;">
                    <thead>
                        <tr style="background: #f8f9fa; text-align: left;">
                            <th style="padding: 0.6rem; border: 1px solid #dee2e6;">Interface</th>
                            <th style="padding: 0.6rem; border: 1px solid #dee2e6;">Ciclo de Vida</th>
                            <th style="padding: 0.6rem; border: 1px solid #dee2e6;">Suporte a Hot Reload</th>
                            <th style="padding: 0.6rem; border: 1px solid #dee2e6;">Valor Lido Atual</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;"><code>IOptions&lt;T&gt;</code></td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Singleton</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Não (valor da subida)</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">{{geral.LimiteItensPorPagina}} itens</td>
                        </tr>
                        <tr>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;"><code>IOptionsSnapshot&lt;T&gt;</code></td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Scoped</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Sim (a cada requisição)</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Dinâmico</td>
                        </tr>
                        <tr>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;"><code>IOptionsMonitor&lt;T&gt;</code></td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Singleton</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;">Sim (notificação imediata)</td>
                            <td style="padding: 0.6rem; border: 1px solid #dee2e6;"><strong>{{geralAtual.LimiteItensPorPagina}} itens</strong></td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="alert-hotreload">
                <strong>Experimente o Recarregamento a Quente:</strong>
                Abra o arquivo <code>appsettings.json</code>, altere o valor de <code>LimiteItensPorPagina</code> e salve.
                Atualize esta página e veja o valor do <code>IOptionsMonitor</code> mudar imediatamente sem reiniciar a aplicação.
            </div>

            <div class="endpoints-box">
                <h3>Endpoints JSON Disponíveis</h3>
                <a class="endpoint-link" href="/api/config/direta" target="_blank">GET /api/config/direta (IConfiguration direto)</a>
                <a class="endpoint-link" href="/api/config/options" target="_blank">GET /api/config/options (IOptions Singleton)</a>
                <a class="endpoint-link" href="/api/config/snapshot" target="_blank">GET /api/config/snapshot (IOptionsSnapshot Scoped)</a>
                <a class="endpoint-link" href="/api/config/monitor" target="_blank">GET /api/config/monitor (IOptionsMonitor Reativo)</a>
                <a class="endpoint-link" href="/api/config/smtp" target="_blank">GET /api/config/smtp (SmtpOptions tipado)</a>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
