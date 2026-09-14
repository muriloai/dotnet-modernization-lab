using JsonSerializationDemo.Models;
using JsonSerializationDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonSerializationDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SerializacaoController : ControllerBase
{
    private readonly SerializacaoBenchmarkService _benchmarkService;

    public SerializacaoController(SerializacaoBenchmarkService benchmarkService)
    {
        _benchmarkService = benchmarkService;
    }

    [HttpPost("polimorfismo")]
    public IActionResult ProcessarPagamento([FromBody] Pagamento pagamento)
    {
        if (pagamento is PagamentoPix pix)
        {
            return Ok(new
            {
                mensagem = "Pagamento via Pix deserializado com sucesso via polimorfismo nativo",
                tipoIdentificado = "Pix",
                valor = pix.Valor,
                chave = pix.ChavePix,
                txid = pix.IdentificadorTransacao,
                dataHora = pix.DataHora
            });
        }

        if (pagamento is PagamentoCartao cartao)
        {
            return Ok(new
            {
                mensagem = "Pagamento via Cartão deserializado com sucesso via polimorfismo nativo",
                tipoIdentificado = "Cartão de Crédito",
                valor = cartao.Valor,
                cartao = cartao.NumeroMascarado,
                parcelas = cartao.Parcelas,
                bandeira = cartao.Bandeira,
                dataHora = cartao.DataHora
            });
        }

        return BadRequest(new { erro = "Tipo de pagamento não reconhecido pelo discriminador polimórfico" });
    }

    [HttpGet("streaming-catalogo")]
    public async IAsyncEnumerable<ProdutoCatalogo> ObterCatalogoStreaming()
    {
        // Demonstra streaming sob demanda com envio fragmentado (Chunked Transfer Encoding)
        for (int i = 1; i <= 25; i++)
        {
            await Task.Delay(60); // Simula leitura fracionada de base de dados ou stream de mensageria
            yield return new ProdutoCatalogo(
                i,
                $"Item de Catálogo em Streaming #{i:D4}",
                199.90m + (i * 1.50m),
                i % 2 == 0 ? "Hardware" : "Software",
                10 + i);
        }
    }

    [HttpGet("benchmark")]
    public IActionResult ExecutarBenchmark()
    {
        var resultado = _benchmarkService.ExecutarBenchmark(2000);
        return Ok(resultado);
    }
}
