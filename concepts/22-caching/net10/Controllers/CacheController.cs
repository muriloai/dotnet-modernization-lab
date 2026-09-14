using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachingDemo.Models;
using CachingDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CachingDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICatalogoService _catalogoService;
        private readonly IOutputCacheStore _outputCacheStore;

        public CacheController(ICatalogoService catalogoService, IOutputCacheStore outputCacheStore)
        {
            _catalogoService = catalogoService;
            _outputCacheStore = outputCacheStore;
        }

        [HttpPost("stampede/sem-trava")]
        public async Task<ActionResult<MetricasStampede>> ExecutarStampedeSemTrava(CancellationToken cancellationToken)
        {
            _catalogoService.LimparCacheLocal();
            _catalogoService.ResetarContadorBanco();

            int numeroRequisicoes = 10;
            var sw = Stopwatch.StartNew();

            var tarefas = Enumerable.Range(1, numeroRequisicoes)
                .Select(_ => _catalogoService.ObterPorCategoriaSemTravaAsync("eletronicos", cancellationToken))
                .ToList();

            var resultados = await Task.WhenAll(tarefas);
            sw.Stop();

            var metricas = new MetricasStampede
            {
                Modo = "Sem Trava (Cache Stampede Clássico)",
                RequisicoesDisparadas = numeroRequisicoes,
                AcessosReaisAoBanco = _catalogoService.TotalAcessosBanco,
                TempoTotalMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                TotalItensRetornados = resultados.FirstOrDefault()?.Count ?? 0,
                Descricao = "Sem controle de concorrência, todas as requisições encontraram cache vazio e dispararam consultas simultâneas contra o banco de dados."
            };

            return Ok(metricas);
        }

        [HttpPost("stampede/com-trava")]
        public async Task<ActionResult<MetricasStampede>> ExecutarStampedeComTrava(CancellationToken cancellationToken)
        {
            _catalogoService.LimparCacheLocal();
            _catalogoService.ResetarContadorBanco();

            int numeroRequisicoes = 10;
            var sw = Stopwatch.StartNew();

            var tarefas = Enumerable.Range(1, numeroRequisicoes)
                .Select(_ => _catalogoService.ObterPorCategoriaComTravaAsync("eletronicos", cancellationToken))
                .ToList();

            var resultados = await Task.WhenAll(tarefas);
            sw.Stop();

            var metricas = new MetricasStampede
            {
                Modo = "Com Trava Assíncrona (Mitigação com SemaphoreSlim)",
                RequisicoesDisparadas = numeroRequisicoes,
                AcessosReaisAoBanco = _catalogoService.TotalAcessosBanco,
                TempoTotalMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                TotalItensRetornados = resultados.FirstOrDefault()?.Count ?? 0,
                Descricao = "Com proteção por chave e dupla verificação, apenas a primeira requisição consultou o banco de dados. As outras aguardaram e compartilharam a resposta."
            };

            return Ok(metricas);
        }

        [HttpPost("stampede/resetar")]
        public IActionResult ResetarMetricas()
        {
            _catalogoService.LimparCacheLocal();
            _catalogoService.ResetarContadorBanco();
            return Ok(new { Mensagem = "Cache em memória e contadores reiniciados com sucesso." });
        }

        [HttpGet("output/categoria/{categoria}")]
        [OutputCache(PolicyName = "PorCategoria")]
        public IActionResult ObterPorCategoriaComOutputCache(string categoria)
        {
            var produtos = _catalogoService.ObterProdutosPorCategoria(categoria);

            return Ok(new
            {
                GeradoEm = DateTime.Now.ToString("HH:mm:ss.fff"),
                Categoria = categoria.ToLowerInvariant(),
                Origem = "Resposta gerada pelo controller (se você visualizar este carimbo repetido em novas requisições, ela está sendo servida pelo middleware de OutputCache)",
                TotalProdutos = produtos.Count,
                Produtos = produtos
            });
        }

        [HttpPost("output/invalidar-tag/{categoria}")]
        public async Task<IActionResult> InvalidarTag(string categoria, CancellationToken cancellationToken)
        {
            string tag = $"categoria-{categoria.ToLowerInvariant()}";
            await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);

            return Ok(new
            {
                Mensagem = $"Tag '{tag}' invalidada com êxito pelo IOutputCacheStore. As próximas consultas a esta categoria gerarão uma nova resposta.",
                TagInvalidada = tag,
                DataHora = DateTime.Now.ToString("HH:mm:ss.fff")
            });
        }

        [HttpGet("distribuido/{id:int}")]
        public async Task<IActionResult> ObterPorIdDistribuido(int id, CancellationToken cancellationToken)
        {
            var produto = await _catalogoService.ObterPorIdDistribuidoAsync(id, cancellationToken);
            if (produto == null)
            {
                return NotFound(new { Mensagem = $"Produto com ID {id} não encontrado." });
            }

            return Ok(new
            {
                Produto = produto,
                Origem = "IDistributedCache (Serializado em UTF-8)",
                DataConsulta = DateTime.Now.ToString("HH:mm:ss.fff")
            });
        }

        [HttpDelete("distribuido/{id:int}")]
        public async Task<IActionResult> RemoverDistribuido(int id, CancellationToken cancellationToken)
        {
            await _catalogoService.InvalidarDistribuidoAsync(id, cancellationToken);
            return Ok(new { Mensagem = $"Chave 'dist_prod_{id}' removida do IDistributedCache." });
        }
    }
}
