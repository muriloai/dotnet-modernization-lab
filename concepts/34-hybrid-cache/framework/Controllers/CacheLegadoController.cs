using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using CacheLegadoDemo.Models;

namespace CacheLegadoDemo.Controllers
{
    public class CacheLegadoController : Controller
    {
        private static long _databaseHits = 0;
        private static readonly MemoryCache _memoryCache = MemoryCache.Default;

        // Simulacao de banco de dados legado
        private static readonly List<ProdutoLegadoDto> _db = new List<ProdutoLegadoDto>
        {
            new ProdutoLegadoDto { Id = 1, Nome = "Servidor PowerEdge R740", Categoria = "hardware", Preco = 18500.00m, Estoque = 4, DataOrigem = DateTime.UtcNow },
            new ProdutoLegadoDto { Id = 2, Nome = "Switch Cisco Catalyst 24P", Categoria = "hardware", Preco = 4900.00m, Estoque = 12, DataOrigem = DateTime.UtcNow },
            new ProdutoLegadoDto { Id = 3, Nome = "NoBreak APC Smart-UPS 3000VA", Categoria = "perifericos", Preco = 3200.00m, Estoque = 8, DataOrigem = DateTime.UtcNow },
            new ProdutoLegadoDto { Id = 4, Nome = "Windows Server 2019 Standard", Categoria = "software", Preco = 3800.00m, Estoque = 50, DataOrigem = DateTime.UtcNow }
        };

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ObterProdutos(string categoria)
        {
            var cat = string.IsNullOrWhiteSpace(categoria) ? "all" : categoria.ToLowerInvariant();
            var cacheKey = "legado:catalog:" + cat;

            var sw = Stopwatch.StartNew();
            var hitsBefore = Interlocked.Read(ref _databaseHits);

            // Padrao legado de MemoryCache: verificar se existe; se nao, buscar e inserir
            // VULNERABILIDADE: Nao ha bloqueio sincronizado nativo para concorrencia!
            var cached = _memoryCache.Get(cacheKey) as List<ProdutoLegadoDto>;
            bool wasDbHit = false;

            if (cached == null)
            {
                wasDbHit = true;
                Interlocked.Increment(ref _databaseHits);

                // Latencia simulada de banco
                Thread.Sleep(200);

                if (cat == "all")
                {
                    cached = _db.Select(p => new ProdutoLegadoDto
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Categoria = p.Categoria,
                        Preco = p.Preco,
                        Estoque = p.Estoque,
                        DataOrigem = DateTime.UtcNow
                    }).ToList();
                }
                else
                {
                    cached = _db.Where(p => p.Categoria.Equals(cat, StringComparison.OrdinalIgnoreCase))
                                .Select(p => new ProdutoLegadoDto
                                {
                                    Id = p.Id,
                                    Nome = p.Nome,
                                    Categoria = p.Categoria,
                                    Preco = p.Preco,
                                    Estoque = p.Estoque,
                                    DataOrigem = DateTime.UtcNow
                                }).ToList();
                }

                // Insercao no cache com expiracao absoluta de 2 minutos (sem suporte a Tags!)
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2)
                };
                _memoryCache.Set(cacheKey, cached, policy);
            }

            sw.Stop();
            var hitsAfter = Interlocked.Read(ref _databaseHits);

            var resultado = new CacheLegadoResultDto
            {
                Dados = cached,
                Origem = wasDbHit ? "Banco de Dados (Cache Miss)" : "System.Runtime.Caching.MemoryCache (Cache Hit)",
                TempoExecucaoMs = sw.ElapsedMilliseconds,
                ContadorConsultasBanco = hitsAfter,
                Mensagem = wasDbHit
                    ? "Cache Miss: Dado recuperado do banco e gravado em MemoryCache sem tags."
                    : "Cache Hit: Dado recuperado do MemoryCache local do processo w3wp."
            };

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SimularCacheStampede(int? count)
        {
            int requisicoes = Math.Min(Math.Max(count ?? 10, 2), 30);
            string stampedeKey = "legado:stampede:" + Guid.NewGuid().ToString("N");
            int execucoesBanco = 0;

            var sw = Stopwatch.StartNew();

            // Dispara tarefas concorrentes sem lock (comportamento tipico do MemoryCache legado)
            var tasks = new Task[requisicoes];
            for (int i = 0; i < requisicoes; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var item = _memoryCache.Get(stampedeKey);
                    if (item == null)
                    {
                        // Todas as requisicoes simultaneas entram aqui ao mesmo tempo!
                        Interlocked.Increment(ref execucoesBanco);
                        Thread.Sleep(250); // Simula query pesada

                        item = "Resultado Banco: " + DateTime.UtcNow.Ticks;
                        var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1) };
                        _memoryCache.Set(stampedeKey, item, policy);
                    }
                });
            }

            Task.WaitAll(tasks);
            sw.Stop();

            _memoryCache.Remove(stampedeKey);

            var result = new StampedeLegadoResultDto
            {
                TotalRequisicoes = requisicoes,
                ExecucoesSemProtecao = execucoesBanco,
                TempoTotalMs = sw.ElapsedMilliseconds,
                Diagnostico = string.Format("Cache Stampede Ocorreu: {0} requisicoes concorrentes executaram a consulta ao banco {1} vezes! O MemoryCache legado nao possui bloqueio concorrente nativo por chave.", requisicoes, execucoesBanco)
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InvalidarPorPrefixo(string prefixo)
        {
            var sw = Stopwatch.StartNew();
            int removidos = 0;

            // No legado, nao existem Tags. E necessario iterar todas as chaves do MemoryCache!
            var chavesParaRemover = _memoryCache.Where(kvp => kvp.Key.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                                                .Select(kvp => kvp.Key)
                                                .ToList();

            foreach (var chave in chavesParaRemover)
            {
                _memoryCache.Remove(chave);
                removidos++;
            }

            sw.Stop();

            return Json(new
            {
                Sucesso = true,
                ItensRemovidos = removidos,
                TempoMs = sw.ElapsedMilliseconds,
                Mensagem = string.Format("Remocao manual por varredura de chaves concluida: {0} chaves removidas.", removidos)
            });
        }
    }
}
