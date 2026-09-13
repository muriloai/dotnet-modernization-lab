using Microsoft.AspNetCore.Mvc;
using HttpClientDemo.Models;
using HttpClientDemo.Services;

namespace HttpClientDemo.Controllers;

/// <summary>
/// Endpoint simulador de API externa de cotações financeiras.
/// Permite simular retornos bem-sucedidos ou falhas transientes (HTTP 503)
/// para demonstrar o comportamento de retries e resiliência dos clientes HTTP.
/// </summary>
[ApiController]
[Route("api/mock-cotacoes")]
[Produces("application/json")]
public sealed class MockCotacoesController : ControllerBase
{
    private readonly ISimuladorFalhasService _simuladorFalhas;

    public MockCotacoesController(ISimuladorFalhasService simuladorFalhas)
    {
        _simuladorFalhas = simuladorFalhas;
    }

    [HttpGet("{moeda}")]
    public IActionResult ObterCotacao(string moeda)
    {
        _simuladorFalhas.RegistrarTentativa();
        var tentativaAtual = _simuladorFalhas.ContadorTentativas;

        if (_simuladorFalhas.SimularFalhaTransiente && tentativaAtual <= _simuladorFalhas.FalhasConsecutivasParaSimular)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = 503,
                Erro = "Serviço temporariamente sobrecarregado (Simulação de Falha Transiente)",
                Tentativa = tentativaAtual,
                Mensagem = $"Falha intencional na tentativa {tentativaAtual} de {_simuladorFalhas.FalhasConsecutivasParaSimular}. Um cliente resiliente executará retry."
            });
        }

        var cotacoes = new Dictionary<string, (string Nome, decimal Valor, decimal Variacao)>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = ("Dólar Comercial Americano", 5.6850m, 0.45m),
            ["EUR"] = ("Euro União Europeia", 6.1820m, -0.22m),
            ["GBP"] = ("Libra Esterlina Britânica", 7.2450m, 0.15m),
            ["BTC"] = ("Bitcoin Criptoativo", 398500.00m, 2.80m)
        };

        if (!cotacoes.TryGetValue(moeda, out var dados))
        {
            return NotFound(new
            {
                Status = 404,
                Erro = "Moeda não suportada",
                Mensagem = $"Cotação para '{moeda}' não foi localizada no catálogo mock."
            });
        }

        var resposta = new CotacaoMoeda(
            Moeda: moeda.ToUpperInvariant(),
            Nome: dados.Nome,
            ValorReais: dados.Valor,
            VariacaoPercentual: dados.Variacao,
            DataHoraUtc: DateTime.UtcNow,
            Fonte: "Mock Central Bank Simulator (.NET 10 Kestrel)"
        );

        return Ok(resposta);
    }

    [HttpPost("configurar-falha")]
    public IActionResult ConfigurarFalha([FromQuery] bool ativar, [FromQuery] int falhas = 2)
    {
        _simuladorFalhas.Resetar();
        _simuladorFalhas.SimularFalhaTransiente = ativar;
        _simuladorFalhas.FalhasConsecutivasParaSimular = falhas;

        return Ok(new
        {
            Mensagem = ativar
                ? $"Simulação ativada: As próximas {falhas} requisições retornarão 503 Service Unavailable antes de responder 200 OK."
                : "Simulação de falhas transientes desativada.",
            SimularFalha = _simuladorFalhas.SimularFalhaTransiente,
            FalhasConsecutivas = _simuladorFalhas.FalhasConsecutivasParaSimular
        });
    }

    [HttpPost("resetar-contador")]
    public IActionResult ResetarContador()
    {
        _simuladorFalhas.Resetar();
        return Ok(new { Mensagem = "Contador de tentativas zerado com sucesso." });
    }
}
