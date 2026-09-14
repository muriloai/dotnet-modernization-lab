using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CachingDemo.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CachingDemo.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private int _totalAcessosBanco;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _travasPorChave = new();

        private static readonly List<Produto> _bancoDadosSimulado = new()
        {
            new Produto(1, "Notebook Ultrafino Pro", "eletronicos", 4500.00m, DateTime.Now),
            new Produto(2, "Monitor IPS 27 polegadas", "eletronicos", 1850.00m, DateTime.Now),
            new Produto(3, "Teclado Mecânico RGB", "eletronicos", 320.00m, DateTime.Now),
            new Produto(4, "Cadeira Ergonômica Executiva", "escritorio", 1200.00m, DateTime.Now),
            new Produto(5, "Mesa com Ajuste Elétrico", "escritorio", 2100.00m, DateTime.Now),
            new Produto(6, "Livro Arquitetura de Software", "livros", 89.90m, DateTime.Now),
            new Produto(7, "Livro Padrões de Projeto", "livros", 110.00m, DateTime.Now)
        };

        public CatalogoService(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        public int TotalAcessosBanco => _totalAcessosBanco;

        public void ResetarContadorBanco()
        {
            Interlocked.Exchange(ref _totalAcessosBanco, 0);
        }

        public void LimparCacheLocal()
        {
            _memoryCache.Remove("cat_eletronicos");
            _memoryCache.Remove("cat_escritorio");
            _memoryCache.Remove("cat_livros");
        }

        public IReadOnlyList<Produto> ObterProdutosPorCategoria(string categoria)
        {
            return _bancoDadosSimulado
                .Where(p => string.Equals(p.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<IReadOnlyList<Produto>> ObterPorCategoriaSemTravaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            string chave = $"cat_{categoria.ToLowerInvariant()}";

            if (_memoryCache.TryGetValue(chave, out IReadOnlyList<Produto>? cached) && cached != null)
            {
                return cached;
            }

            // Simulação de latência de banco de dados
            await Task.Delay(300, cancellationToken);
            Interlocked.Increment(ref _totalAcessosBanco);

            var resultado = ObterProdutosPorCategoria(categoria);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15)
            };

            _memoryCache.Set(chave, resultado, options);
            return resultado;
        }

        public async Task<IReadOnlyList<Produto>> ObterPorCategoriaComTravaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            string chave = $"cat_{categoria.ToLowerInvariant()}";

            if (_memoryCache.TryGetValue(chave, out IReadOnlyList<Produto>? cached) && cached != null)
            {
                return cached;
            }

            var semaforo = _travasPorChave.GetOrAdd(chave, _ => new SemaphoreSlim(1, 1));
            await semaforo.WaitAsync(cancellationToken);

            try
            {
                // Dupla verificação após adquirir a trava
                if (_memoryCache.TryGetValue(chave, out cached) && cached != null)
                {
                    return cached;
                }

                // Apenas uma thread executa a consulta ao banco sob expiração
                await Task.Delay(300, cancellationToken);
                Interlocked.Increment(ref _totalAcessosBanco);

                var resultado = ObterProdutosPorCategoria(categoria);

                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15)
                };

                _memoryCache.Set(chave, resultado, options);
                return resultado;
            }
            finally
            {
                semaforo.Release();
            }
        }

        public async Task<Produto?> ObterPorIdDistribuidoAsync(int id, CancellationToken cancellationToken = default)
        {
            string chave = $"dist_prod_{id}";

            byte[]? dados = await _distributedCache.GetAsync(chave, cancellationToken);
            if (dados != null)
            {
                return JsonSerializer.Deserialize<Produto>(dados);
            }

            // Simula latência de consulta
            await Task.Delay(200, cancellationToken);
            var produto = _bancoDadosSimulado.FirstOrDefault(p => p.Id == id);

            if (produto != null)
            {
                byte[] bytesParaSalvar = JsonSerializer.SerializeToUtf8Bytes(produto);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                    SlidingExpiration = TimeSpan.FromSeconds(30)
                };

                await _distributedCache.SetAsync(chave, bytesParaSalvar, options, cancellationToken);
            }

            return produto;
        }

        public async Task InvalidarDistribuidoAsync(int id, CancellationToken cancellationToken = default)
        {
            string chave = $"dist_prod_{id}";
            await _distributedCache.RemoveAsync(chave, cancellationToken);
        }
    }
}
