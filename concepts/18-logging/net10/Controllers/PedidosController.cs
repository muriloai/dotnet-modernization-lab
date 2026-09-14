using LoggingDemo.Models;
using LoggingDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoggingDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly ILogger<PedidosController> _logger;
    private readonly ProcessadorPedidosService _processador;

    public PedidosController(
        ILogger<PedidosController> logger,
        ProcessadorPedidosService processador)
    {
        _logger = logger;
        _processador = processador;
    }

    [HttpPost("processar")]
    public IActionResult Processar([FromBody] RequisicaoProcessamento requisicao)
    {
        var correlationId = Request.Headers.TryGetValue("X-Correlation-ID", out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
            ? headerVal.ToString()
            : $"REQ-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["OrigemAction"] = nameof(Processar)
        }))
        {
            _logger.LogInformation(
                "Recebida solicitação de processamento para o pedido {PedidoId} via API REST",
                requisicao.PedidoId);

            _processador.Processar(requisicao, correlationId);

            return Ok(new
            {
                mensagem = "Processamento executado",
                correlationId,
                pedidoId = requisicao.PedidoId,
                status = requisicao.SimularErro ? "Falha simulada" : "Concluído com sucesso"
            });
        }
    }
}
