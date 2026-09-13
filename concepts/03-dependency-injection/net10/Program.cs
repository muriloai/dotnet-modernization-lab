using DependencyInjectionDemo.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------------------
// 1. Registro dos Ciclos de Vida (Lifetimes) no Container Nativo
// -------------------------------------------------------------------------------------
// Transient: nova instância criada a cada vez que o serviço é solicitado
builder.Services.AddTransient<ITransientService, TransientService>();

// Scoped: mesma instância compartilhada durante toda a requisição HTTP atual
builder.Services.AddScoped<IScopedService, ScopedService>();

// Singleton: única instância instanciada uma vez e compartilhada por toda a aplicação
builder.Services.AddSingleton<ISingletonService, SingletonService>();

// -------------------------------------------------------------------------------------
// 2. Serviço de Domínio com Primary Constructor
// -------------------------------------------------------------------------------------
builder.Services.AddScoped<IClienteService, ClienteService>();

// -------------------------------------------------------------------------------------
// 3. Keyed Services (recurso nativo desde o .NET 8, aprimorado no .NET 10)
// -------------------------------------------------------------------------------------
// Registra duas implementações da mesma interface INotificacaoService diferenciadas por chave nominal
builder.Services.AddKeyedScoped<INotificacaoService, EmailNotificacaoService>("email");
builder.Services.AddKeyedScoped<INotificacaoService, SmsNotificacaoService>("sms");

var app = builder.Build();

app.UseStaticFiles();

// -------------------------------------------------------------------------------------
// 4. Endpoints Didáticos
// -------------------------------------------------------------------------------------

// Endpoint 1: Comparativo de Ciclos de Vida dentro da MESMA requisição HTTP
// Recebe duas injeções de cada ciclo de vida para comprovar o comportamento dos GUIDs
app.MapGet("/api/di/lifetimes", (
    ITransientService transient1,
    ITransientService transient2,
    IScopedService scoped1,
    IScopedService scoped2,
    ISingletonService singleton1,
    ISingletonService singleton2) =>
{
    return Results.Ok(new
    {
        Explicacao = "Comparação de duas resoluções para cada ciclo de vida dentro da mesma requisição HTTP.",
        Transient = new
        {
            Descricao = "Gera um novo GUID a cada injeção.",
            Instancia1 = transient1.Id,
            Instancia2 = transient2.Id,
            SaoIguaisNaMesmaRequisicao = transient1.Id == transient2.Id
        },
        Scoped = new
        {
            Descricao = "Mantém o mesmo GUID em toda a mesma requisição HTTP.",
            Instancia1 = scoped1.Id,
            Instancia2 = scoped2.Id,
            SaoIguaisNaMesmaRequisicao = scoped1.Id == scoped2.Id
        },
        Singleton = new
        {
            Descricao = "Mantém o mesmo GUID por todo o ciclo de vida da aplicação.",
            Instancia1 = singleton1.Id,
            Instancia2 = singleton2.Id,
            SaoIguaisNaMesmaRequisicao = singleton1.Id == singleton2.Id
        }
    });
});

// Endpoint 2: Demonstração de Keyed Services (resolução por chave nominal)
app.MapGet("/api/di/keyed", (
    [FromKeyedServices("email")] INotificacaoService servicoEmail,
    [FromKeyedServices("sms")] INotificacaoService servicoSms) =>
{
    return Results.Ok(new
    {
        Descricao = "Duas implementações distintas de INotificacaoService resolvidas via [FromKeyedServices].",
        NotificacaoEmail = new
        {
            Canal = servicoEmail.Canal,
            Resultado = servicoEmail.Enviar("cliente@exemplo.com", "Sua fatura foi fechada.")
        },
        NotificacaoSms = new
        {
            Canal = servicoSms.Canal,
            Resultado = servicoSms.Enviar("(11) 98888-7777", "Código de autenticação: 928341.")
        }
    });
});

// Endpoint 3: Serviço de Clientes injetado
app.MapGet("/api/di/clientes", (IClienteService clienteService) =>
{
    return Results.Ok(clienteService.ObterTodos());
});

// Endpoint Raiz: Dashboard HTML Didático
app.MapGet("/", (
    ITransientService t1,
    ITransientService t2,
    IScopedService s1,
    IScopedService s2,
    ISingletonService u1,
    ISingletonService u2,
    [FromKeyedServices("email")] INotificacaoService servicoEmail,
    [FromKeyedServices("sms")] INotificacaoService servicoSms) =>
{
    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Lab 03: Injeção de Dependências (.NET 10)</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 03: Injeção de Dependências</h1>
            <p class="subtitle">Demonstração prática dos ciclos de vida (Transient, Scoped, Singleton) e Keyed Services.</p>

            <div class="grid">
                <div class="card">
                    <h2>Ciclos de Vida: Transient</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">Uma nova instância é gerada a cada injeção. Os identificadores serão diferentes mesmo dentro da mesma página.</p>
                    <ul class="feature-list">
                        <li><span>Injeção 1:</span> <span class="guid-tag guid-diff">{{t1.Id}}</span></li>
                        <li><span>Injeção 2:</span> <span class="guid-tag guid-diff">{{t2.Id}}</span></li>
                        <li><span>São iguais?</span> <strong>{{(t1.Id == t2.Id ? "Sim" : "Não (Correto)")}}</strong></li>
                    </ul>
                </div>

                <div class="card">
                    <h2>Ciclos de Vida: Scoped</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">Mesma instância durante toda a requisição atual. Os identificadores serão idênticos nesta requisição, mas mudarão se você recarregar a página (F5).</p>
                    <ul class="feature-list">
                        <li><span>Injeção 1:</span> <span class="guid-tag guid-same">{{s1.Id}}</span></li>
                        <li><span>Injeção 2:</span> <span class="guid-tag guid-same">{{s2.Id}}</span></li>
                        <li><span>São iguais?</span> <strong>{{(s1.Id == s2.Id ? "Sim (Correto)" : "Não")}}</strong></li>
                    </ul>
                </div>

                <div class="card">
                    <h2>Ciclos de Vida: Singleton</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">Uma única instância durante toda a vida útil da aplicação. O identificador nunca muda entre requisições.</p>
                    <ul class="feature-list">
                        <li><span>Injeção 1:</span> <span class="guid-tag guid-same">{{u1.Id}}</span></li>
                        <li><span>Injeção 2:</span> <span class="guid-tag guid-same">{{u2.Id}}</span></li>
                        <li><span>São iguais?</span> <strong>{{(u1.Id == u2.Id ? "Sim (Correto)" : "Não")}}</strong></li>
                    </ul>
                </div>

                <div class="card">
                    <h2>Keyed Services (.NET 8/10)</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">Resolução de múltiplas implementações de INotificacaoService usando chaves nominais ("email" e "sms").</p>
                    <ul class="feature-list">
                        <li><span>Chave "email":</span> <span class="guid-tag">{{servicoEmail.Canal}}</span></li>
                        <li><span>Chave "sms":</span> <span class="guid-tag">{{servicoSms.Canal}}</span></li>
                    </ul>
                    <p style="margin-top: 0.5rem; font-size: 0.85rem; color: #495057;">Disponibilizado com [FromKeyedServices("nome")] direto nos parâmetros do endpoint.</p>
                </div>
            </div>

            <div class="note-box">
                <strong>Experimente o comportamento:</strong>
                Pressione <strong>F5</strong> para recarregar a página.
                Observe que os GUIDs do <strong>Transient</strong> e do <strong>Scoped</strong> mudam a cada requisição, enquanto os GUIDs do <strong>Singleton</strong> permanecem estritamente os mesmos.
            </div>

            <div class="endpoints-box">
                <h3>Endpoints JSON Disponíveis</h3>
                <a class="endpoint-link" href="/api/di/lifetimes" target="_blank">GET /api/di/lifetimes (Comparativo de GUIDs no JSON)</a>
                <a class="endpoint-link" href="/api/di/keyed" target="_blank">GET /api/di/keyed (Keyed Services em ação)</a>
                <a class="endpoint-link" href="/api/di/clientes" target="_blank">GET /api/di/clientes (Serviço com Primary Constructor)</a>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
