using HybridCacheDemo.Models;

namespace HybridCacheDemo.Services;

public class ProductCatalogService
{
    private static long _databaseQueryCount = 0;
    private static long _factoryExecutionCount = 0;

    private readonly List<ProductDto> _database = new()
    {
        new(1, "Notebook Dell XPS 15", "hardware", 8999.00m, 14, DateTime.UtcNow),
        new(2, "Monitor Dell UltraSharp 27", "hardware", 3299.00m, 22, DateTime.UtcNow),
        new(3, "Teclado Mecanico MX Keys", "perifericos", 649.00m, 45, DateTime.UtcNow),
        new(4, "Mouse Logitech MX Master 3S", "perifericos", 589.00m, 38, DateTime.UtcNow),
        new(5, "Licenca Microsoft 365 Business", "software", 120.00m, 100, DateTime.UtcNow),
        new(6, "Licenca Visual Studio Enterprise", "software", 2499.00m, 15, DateTime.UtcNow)
    };

    public long DatabaseQueryCount => Interlocked.Read(ref _databaseQueryCount);
    public long FactoryExecutionCount => Interlocked.Read(ref _factoryExecutionCount);

    public async Task<List<ProductDto>> GetProductsFromDatabaseAsync(string? categoria, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _databaseQueryCount);
        Interlocked.Increment(ref _factoryExecutionCount);

        // Simula latencia real de I/O de banco de dados (200ms)
        await Task.Delay(200, ct);

        if (string.IsNullOrWhiteSpace(categoria) || categoria.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return _database.Select(p => p with { DataOrigem = DateTime.UtcNow }).ToList();
        }

        return _database
            .Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
            .Select(p => p with { DataOrigem = DateTime.UtcNow })
            .ToList();
    }

    public async Task<ProductDto?> GetProductByIdFromDatabaseAsync(int id, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _databaseQueryCount);
        Interlocked.Increment(ref _factoryExecutionCount);

        // Simula latencia de consulta pontual no banco de dados (150ms)
        await Task.Delay(150, ct);

        var item = _database.FirstOrDefault(p => p.Id == id);
        return item != null ? item with { DataOrigem = DateTime.UtcNow } : null;
    }

    public void ResetCounters()
    {
        Interlocked.Exchange(ref _databaseQueryCount, 0);
        Interlocked.Exchange(ref _factoryExecutionCount, 0);
    }
}
