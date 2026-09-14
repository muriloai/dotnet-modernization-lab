using System.Diagnostics;
using HybridCacheDemo.Models;
using HybridCacheDemo.Services;
using Microsoft.Extensions.Caching.Hybrid;

#pragma warning disable EXTEXP0018

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProductCatalogService>();

// Registra HybridCache do .NET 10 (L1 Memoria Local + L2 Distribuido)
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(2),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint: Listagem de produtos com HybridCache e Tags
app.MapGet("/api/products", async (string? categoria, HybridCache cache, ProductCatalogService catalogService, CancellationToken ct) =>
{
    var cat = string.IsNullOrWhiteSpace(categoria) ? "all" : categoria.ToLowerInvariant();
    var cacheKey = $"catalog:products:{cat}";
    var tags = new[] { "products", $"category:{cat}" };

    var sw = Stopwatch.StartNew();
    var dbCountBefore = catalogService.DatabaseQueryCount;

    var products = await cache.GetOrCreateAsync(
        cacheKey,
        async cancelToken => await catalogService.GetProductsFromDatabaseAsync(cat, cancelToken),
        tags: tags,
        cancellationToken: ct
    );

    sw.Stop();
    var dbCountAfter = catalogService.DatabaseQueryCount;
    var wasFromDb = dbCountAfter > dbCountBefore;

    return Results.Ok(new CacheResultDto<List<ProductDto>>(
        products,
        wasFromDb ? "Banco de Dados (Cache Miss)" : "HybridCache L1/L2 (Cache Hit)",
        sw.ElapsedMilliseconds,
        dbCountAfter,
        wasFromDb
            ? "Item gerado pelo banco de dados e gravado automaticamente no HybridCache com tags."
            : "Item recuperado em memoria de alta velocidade sem consultar o banco de dados."
    ));
});

// Endpoint: Consulta pontual com tag especifica
app.MapGet("/api/products/{id:int}", async (int id, HybridCache cache, ProductCatalogService catalogService, CancellationToken ct) =>
{
    var cacheKey = $"catalog:product:{id}";
    var sw = Stopwatch.StartNew();
    var dbCountBefore = catalogService.DatabaseQueryCount;

    var product = await cache.GetOrCreateAsync(
        cacheKey,
        async cancelToken => await catalogService.GetProductByIdFromDatabaseAsync(id, cancelToken),
        tags: new[] { "products", $"product:{id}" },
        cancellationToken: ct
    );

    sw.Stop();
    if (product == null)
    {
        return Results.NotFound(new { Mensagem = $"Produto com Id {id} nao encontrado." });
    }

    var dbCountAfter = catalogService.DatabaseQueryCount;
    var wasFromDb = dbCountAfter > dbCountBefore;

    return Results.Ok(new CacheResultDto<ProductDto>(
        product,
        wasFromDb ? "Banco de Dados (Cache Miss)" : "HybridCache L1/L2 (Cache Hit)",
        sw.ElapsedMilliseconds,
        dbCountAfter,
        wasFromDb
            ? $"Produto {id} recuperado do banco e armazenado no cache."
            : $"Produto {id} servido instantaneamente a partir do cache."
    ));
});

// Endpoint: Invalidacao seletiva por Tag (Remocao em lote por categoria)
app.MapPost("/api/cache/invalidate-tag", async (string tag, HybridCache cache) =>
{
    if (string.IsNullOrWhiteSpace(tag))
    {
        return Results.BadRequest(new { Sucesso = false, Mensagem = "Tag de invalidacao nao informada." });
    }

    var sw = Stopwatch.StartNew();
    await cache.RemoveByTagAsync(tag);
    sw.Stop();

    return Results.Ok(new
    {
        Sucesso = true,
        TagRemovida = tag,
        TempoMs = sw.ElapsedMilliseconds,
        Mensagem = $"Todos os itens associados a tag '{tag}' foram invalidados atomicamente."
    });
});

// Endpoint: Invalidacao direta por chave
app.MapPost("/api/cache/invalidate-key", async (string key, HybridCache cache) =>
{
    if (string.IsNullOrWhiteSpace(key))
    {
        return Results.BadRequest(new { Sucesso = false, Mensagem = "Chave nao informada." });
    }

    var sw = Stopwatch.StartNew();
    await cache.RemoveAsync(key);
    sw.Stop();

    return Results.Ok(new
    {
        Sucesso = true,
        ChaveRemovida = key,
        TempoMs = sw.ElapsedMilliseconds,
        Mensagem = $"Chave '{key}' removida com sucesso."
    });
});

// Endpoint: Demonstracao de Protecao contra Cache Stampede (Thundering Herd)
app.MapGet("/api/cache/stampede-test", async (int? count, HybridCache cache, ProductCatalogService catalogService) =>
{
    var requisicoes = Math.Clamp(count ?? 10, 2, 50);
    var stampedeKey = $"stampede-key-{Guid.NewGuid():N}";
    var factoryRuns = 0;

    var sw = Stopwatch.StartNew();

    // Dispara N requisicoes concorrentes simultaneas para a MESMA chave ausente
    var tasks = Enumerable.Range(1, requisicoes).Select(async i =>
    {
        return await cache.GetOrCreateAsync(
            stampedeKey,
            async ct =>
            {
                Interlocked.Increment(ref factoryRuns);
                // Simula operacao cara de banco ou calculo (300ms)
                await Task.Delay(300, ct);
                return $"Dados Calculados - Id: {Guid.NewGuid():N}";
            }
        );
    });

    var results = await Task.WhenAll(tasks);
    sw.Stop();

    // Limpa a chave do teste
    await cache.RemoveAsync(stampedeKey);

    return Results.Ok(new StampedeTestResult(
        requisicoes,
        factoryRuns,
        results.Length,
        sw.ElapsedMilliseconds,
        factoryRuns == 1
            ? $"Sucesso: {requisicoes} requisicoes concorrentes simultaneas executaram a factory exatamente 1 vez. O HybridCache evitou completamente o Cache Stampede!"
            : $"Atencao: a factory executou {factoryRuns} vezes."
    ));
});

// Estatisticas gerais e reset
app.MapGet("/api/cache/stats", (ProductCatalogService catalogService) =>
{
    return Results.Ok(new
    {
        ConsultasBanco = catalogService.DatabaseQueryCount,
        ExecucoesFactory = catalogService.FactoryExecutionCount
    });
});

app.MapPost("/api/cache/reset-stats", (ProductCatalogService catalogService) =>
{
    catalogService.ResetCounters();
    return Results.Ok(new { Sucesso = true, Mensagem = "Contadores reiniciados com sucesso." });
});

app.Run();
