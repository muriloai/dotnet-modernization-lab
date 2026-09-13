using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using HttpClientDemo.Models;
using HttpClientDemo.Services;

namespace HttpClientDemo.Controllers;

/// <summary>
/// Controller didático que compara os 4 padrões modernos de consumo de APIs HTTP:
/// 1. Basic Client (IHttpClientFactory.CreateClient())
/// 2. Named Client (IHttpClientFactory.CreateClient("Nome"))
/// 3. Typed Client (Injetado diretamente como serviço tipado)
/// 4. Resilient Client (Com pipeline de retry automático)
/// </summary>
[ApiController]
[Route("api/clientes-http")]
[Produces("application/json")]
public sealed class ClientesHttpController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ICotacaoService _cotacaoService;
    private readonly ISimuladorFalhasService _simuladorFalhas;

    public ClientesHttpController(
        IHttpClientFactory clientFactory,
        ICotacaoService cotacaoService,
        ISimuladorFalhasService simuladorFalhas)
    {
        _clientFactory = clientFactory;
        _cotacaoService = cotacaoService;
        _simuladorFalhas = simuladorFalhas;
    }

    /// <summary>
    /// Padrão 1: Basic Client criado via IHttpClientFactory.CreateClient().
    /// Os SocketsHttpHandlers são reaproveitados do pool interno, evitando socket exhaustion.
    /// </summary>
    [HttpGet("basico/{moeda}")]
    public async Task<ActionResult<MetricaConexao>> TestarBasicClient(string moeda)
    {
        var client = _clientFactory.CreateClient();
        var sw = Stopwatch.StartNew();

        try
        {
            var url = $"http://localhost:6500/api/mock-cotacoes/{moeda}";
            var cotacao = await client.GetFromJsonAsync<CotacaoMoeda>(url);
            sw.Stop();

            return Ok(new MetricaConexao(
                ModoCliente: "Basic Client (IHttpClientFactory.CreateClient())",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: StatusCodes.Status200OK,
                TentativasRealizadas: 1,
                Mensagem: "Cliente básico instanciado sob demanda com handlers reaproveitados do pool gerenciado.",
                Dados: cotacao
            ));
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            var status = (int?)ex.StatusCode ?? 500;
            return StatusCode(status, new MetricaConexao(
                ModoCliente: "Basic Client",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: status,
                TentativasRealizadas: 1,
                Mensagem: $"Falha HTTP: {ex.Message}",
                Dados: null
            ));
        }
    }

    /// <summary>
    /// Padrão 2: Named Client configurado com BaseAddress e cabeçalhos centralizados no Program.cs.
    /// </summary>
    [HttpGet("nomeado/{moeda}")]
    public async Task<ActionResult<MetricaConexao>> TestarNamedClient(string moeda)
    {
        var client = _clientFactory.CreateClient("CotacoesNomeado");
        var sw = Stopwatch.StartNew();

        try
        {
            var cotacao = await client.GetFromJsonAsync<CotacaoMoeda>($"api/mock-cotacoes/{moeda}");
            sw.Stop();

            return Ok(new MetricaConexao(
                ModoCliente: "Named Client (CreateClient(\"CotacoesNomeado\"))",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: StatusCodes.Status200OK,
                TentativasRealizadas: 1,
                Mensagem: "Cliente nomeado com BaseAddress e cabeçalhos pré-configurados no container.",
                Dados: cotacao
            ));
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            var status = (int?)ex.StatusCode ?? 500;
            return StatusCode(status, new MetricaConexao(
                ModoCliente: "Named Client",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: status,
                TentativasRealizadas: 1,
                Mensagem: $"Falha HTTP: {ex.Message}",
                Dados: null
            ));
        }
    }

    /// <summary>
    /// Padrão 3: Typed Client encapsulado em classe de serviço própria (ICotacaoService).
    /// </summary>
    [HttpGet("tipado/{moeda}")]
    public async Task<ActionResult<MetricaConexao>> TestarTypedClient(string moeda)
    {
        var resultado = await _cotacaoService.ObterCotacaoTypedClientAsync(moeda);
        return Ok(resultado);
    }

    /// <summary>
    /// Padrão 4: Resilient Client configurado com políticas de retry e circuit breaker.
    /// Em caso de falha transiente (ex: 503), o pipeline executa retries automáticos com backoff.
    /// </summary>
    [HttpGet("resiliente/{moeda}")]
    public async Task<ActionResult<MetricaConexao>> TestarResilientClient(string moeda)
    {
        var client = _clientFactory.CreateClient("CotacoesResiliente");
        var sw = Stopwatch.StartNew();
        var tentativasIniciais = _simuladorFalhas.ContadorTentativas;

        try
        {
            var cotacao = await client.GetFromJsonAsync<CotacaoMoeda>($"api/mock-cotacoes/{moeda}");
            sw.Stop();

            var tentativasExecutadas = Math.Max(1, _simuladorFalhas.ContadorTentativas - tentativasIniciais);

            return Ok(new MetricaConexao(
                ModoCliente: "Resilient Client (Polly / Standard Resilience Handler)",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: StatusCodes.Status200OK,
                TentativasRealizadas: tentativasExecutadas,
                Mensagem: tentativasExecutadas > 1
                    ? $"Sucesso após resiliência: O cliente recuperou-se automaticamente após {tentativasExecutadas - 1} falhas transientes!"
                    : "Sucesso na primeira tentativa sem necessidade de retries.",
                Dados: cotacao
            ));
        }
        catch (Exception ex)
        {
            sw.Stop();
            var tentativasExecutadas = Math.Max(1, _simuladorFalhas.ContadorTentativas - tentativasIniciais);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new MetricaConexao(
                ModoCliente: "Resilient Client",
                TempoRespostaMs: sw.ElapsedMilliseconds,
                StatusCode: StatusCodes.Status503ServiceUnavailable,
                TentativasRealizadas: tentativasExecutadas,
                Mensagem: $"Pipeline de resiliência esgotou as tentativas configuradas: {ex.Message}",
                Dados: null
            ));
        }
    }
}
