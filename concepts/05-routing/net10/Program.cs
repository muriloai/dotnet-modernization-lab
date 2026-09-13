using RoutingDemo.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticFiles();

// -------------------------------------------------------------------------------------
// Base de dados mockada para o laboratório de roteamento
// -------------------------------------------------------------------------------------
var produtos = new List<Produto>
{
    new(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
    new(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true),
    new(3, "Teclado Mecânico RGB", "perifericos", 350.00m, false),
    new(4, "Licença Visual Studio Professional", "software", 2500.00m, true)
};

// -------------------------------------------------------------------------------------
// 1. Grupos de Rotas (MapGroup) no .NET 10
// -------------------------------------------------------------------------------------
// O MapGroup permite prefixar rotas e aplicar metadados/filtros de forma centralizada,
// sem a necessidade de duplicar strings ou classes controladoras isoladas.
var apiProdutos = app.MapGroup("/api/produtos");

// Rota 1: Lista todos os produtos
apiProdutos.MapGet("/", () => Results.Ok(produtos))
    .WithName("ListarProdutos");

// Rota 2: Busca por ID com restrição inline numérica {id:int}
// Se o cliente enviar /api/produtos/abc, o Kestrel retorna 404 automaticamente
apiProdutos.MapGet("/{id:int}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    return produto is not null ? Results.Ok(produto) : Results.NotFound(new { Mensagem = $"Produto com Id {id} não encontrado." });
})
.WithName("ObterProdutoPorId");

// Rota 3: Busca por categoria com restrição alfabética {categoria:alpha}
apiProdutos.MapGet("/categoria/{categoria:alpha}", (string categoria) =>
{
    var filtrados = produtos.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(new
    {
        CategoriaBuscada = categoria,
        TotalEncontrado = filtrados.Count,
        Produtos = filtrados
    });
})
.WithName("ObterPorCategoria");

// -------------------------------------------------------------------------------------
// 2. Geração de URLs com LinkGenerator (Desacoplado do HttpContext)
// -------------------------------------------------------------------------------------
app.MapGet("/api/links", (LinkGenerator linkGenerator, HttpContext context) =>
{
    var linkProduto1 = linkGenerator.GetUriByName(context, "ObterProdutoPorId", new { id = 1 });
    var linkCategoria = linkGenerator.GetUriByName(context, "ObterPorCategoria", new { categoria = "hardware" });
    var linkLista = linkGenerator.GetUriByName(context, "ListarProdutos");

    return Results.Ok(new
    {
        Descricao = "URLs geradas via serviço LinkGenerator do ASP.NET Core.",
        LinkProduto1 = linkProduto1,
        LinkCategoriaHardware = linkCategoria,
        LinkListaProdutos = linkLista
    });
});

// -------------------------------------------------------------------------------------
// 3. Rota Raiz: Dashboard HTML Didático
// -------------------------------------------------------------------------------------
app.MapGet("/", (LinkGenerator linkGenerator, HttpContext context) =>
{
    var urlProduto1 = linkGenerator.GetPathByName("ObterProdutoPorId", new { id = 1 });
    var urlCategoria = linkGenerator.GetPathByName("ObterPorCategoria", new { categoria = "hardware" });

    var html = $$"""
    <!DOCTYPE html>
    <html lang="pt-BR">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Lab 05: Roteamento (.NET 10)</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <div class="container">
            <span class="badge">Ambiente Moderno: .NET 10 (LTS)</span>
            <h1>Lab 05: Roteamento com Endpoint Routing</h1>
            <p class="subtitle">Demonstração de tabela de rotas unificada, MapGroup, restrições inline e LinkGenerator.</p>

            <div class="grid">
                <div class="card">
                    <h2>Grupos de Rotas (MapGroup)</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">
                        Agrupamento centralizado sob o prefixo <code>/api/produtos</code>.
                    </p>
                    <ul class="feature-list">
                        <li><span>GET /api/produtos:</span> <span class="route-badge">Todos os itens</span></li>
                        <li><span>GET /api/produtos/{id:int}:</span> <span class="route-badge">Constraint numérica</span></li>
                        <li><span>GET /api/produtos/categoria/{c:alpha}:</span> <span class="route-badge">Constraint alfabética</span></li>
                    </ul>
                </div>

                <div class="card">
                    <h2>Geração Tipada com LinkGenerator</h2>
                    <p style="font-size: 0.9rem; color: #6c757d; margin-bottom: 0.75rem;">
                        Geração de URLs canônicas seguras via injeção de dependências:
                    </p>
                    <ul class="feature-list">
                        <li><span>Produto 1:</span> <span class="tag">{{urlProduto1}}</span></li>
                        <li><span>Categoria Hardware:</span> <span class="tag">{{urlCategoria}}</span></li>
                    </ul>
                    <p style="margin-top: 0.75rem; font-size: 0.85rem; color: #495057;">
                        Não depende de <code>Url.Action()</code> nem de acoplamento à View.
                    </p>
                </div>
            </div>

            <div class="card" style="margin-bottom: 1.5rem;">
                <h2>Teste Prático de Restrições de Rota (Constraints)</h2>
                <table style="width: 100%; border-collapse: collapse; font-size: 0.9rem;">
                    <thead>
                        <tr style="background: #f8f9fa; text-align: left;">
                            <th style="padding: 0.5rem; border: 1px solid #dee2e6;">URL de Teste</th>
                            <th style="padding: 0.5rem; border: 1px solid #dee2e6;">Restrição Aplicada</th>
                            <th style="padding: 0.5rem; border: 1px solid #dee2e6;">Resultado Esperado</th>
                            <th style="padding: 0.5rem; border: 1px solid #dee2e6;">Testar</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>/api/produtos/1</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>{id:int}</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6; color: green;">200 OK (Válido)</td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><a href="/api/produtos/1" target="_blank">Acessar</a></td>
                        </tr>
                        <tr>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>/api/produtos/abc</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>{id:int}</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6; color: red;">404 Not Found (Constraint Rejeitou)</td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><a href="/api/produtos/abc" target="_blank">Acessar</a></td>
                        </tr>
                        <tr>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>/api/produtos/categoria/hardware</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>{categoria:alpha}</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6; color: green;">200 OK (Válido)</td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><a href="/api/produtos/categoria/hardware" target="_blank">Acessar</a></td>
                        </tr>
                        <tr>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>/api/produtos/categoria/123</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><code>{categoria:alpha}</code></td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6; color: red;">404 Not Found (Constraint Rejeitou)</td>
                            <td style="padding: 0.5rem; border: 1px solid #dee2e6;"><a href="/api/produtos/categoria/123" target="_blank">Acessar</a></td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="endpoints-box">
                <h3>Endpoints Disponíveis</h3>
                <a class="endpoint-link" href="/api/produtos" target="_blank">GET /api/produtos (Listar todos)</a>
                <a class="endpoint-link" href="/api/links" target="_blank">GET /api/links (Geração com LinkGenerator)</a>
            </div>
        </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
