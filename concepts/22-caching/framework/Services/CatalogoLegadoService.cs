using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using CachingDemo.Models;

namespace CachingDemo.Services
{
    public class CatalogoLegadoService
    {
        private static int _totalAcessosBanco = 0;

        private static readonly List<ProdutoLegado> _bancoSimulado = new List<ProdutoLegado>
        {
            new ProdutoLegado(1, "Notebook Legado Corporate", "eletronicos", 3800.00m),
            new ProdutoLegado(2, "Monitor VGA/DVI 24 polegadas", "eletronicos", 950.00m),
            new ProdutoLegado(3, "Teclado PS/2 Padrão ABNT2", "eletronicos", 65.00m),
            new ProdutoLegado(4, "Cadeira Giratória Diretor", "escritorio", 800.00m),
            new ProdutoLegado(5, "Mesa em L em MDP", "escritorio", 600.00m)
        };

        public int TotalAcessosBanco
        {
            get { return _totalAcessosBanco; }
        }

        public void ResetarContadorBanco()
        {
            Interlocked.Exchange(ref _totalAcessosBanco, 0);
        }

        public void LimparCache(string chave)
        {
            if (HttpRuntime.Cache != null)
            {
                HttpRuntime.Cache.Remove(chave);
            }
        }

        public List<ProdutoLegado> ObterPorCategoriaSemProtecao(string categoria)
        {
            string chave = "legado_cat_" + categoria.ToLowerInvariant();
            var cache = HttpRuntime.Cache;

            var dados = cache != null ? cache.Get(chave) as List<ProdutoLegado> : null;
            if (dados != null)
            {
                return dados;
            }

            // Simulação de latência de banco de dados (200 ms)
            Thread.Sleep(200);
            Interlocked.Increment(ref _totalAcessosBanco);

            var resultado = _bancoSimulado
                .Where(p => string.Equals(p.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (cache != null)
            {
                // Inserção com expiração absoluta de 30 segundos
                cache.Insert(
                    chave,
                    resultado,
                    null,
                    DateTime.Now.AddSeconds(30),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }

            return resultado;
        }

        public List<ProdutoLegado> ObterTodosDireto()
        {
            return _bancoSimulado.ToList();
        }
    }
}
