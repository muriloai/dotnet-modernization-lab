using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Hubs;
using SignalRDemo.Models;
using SignalRDemo.Services;

namespace SignalRDemo.Controllers;

/// <summary>
/// Controller REST que demonstra o envio de notificações push em tempo real
/// para os clientes SignalR conectados via injeção de IHubContext.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class NotificacoesController : ControllerBase
{
    private readonly ITransmissorEventos _transmissorEventos;

    public NotificacoesController(ITransmissorEventos transmissorEventos)
    {
        _transmissorEventos = transmissorEventos;
    }

    /// <summary>
    /// Endpoint HTTP convencional que dispara uma notificação push para os clientes conectados no Hub.
    /// </summary>
    [HttpPost("disparar-rest")]
    public async Task<IActionResult> DispararPorRest([FromBody] EnviarAvisoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Mensagem))
        {
            return BadRequest(new { Erro = "Título e mensagem são campos obrigatórios." });
        }

        await _transmissorEventos.DispararNotificacaoExternaAsync(request);

        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = "Evento transmitido com êxito para os clientes conectados no SignalR.",
            Dados = request
        });
    }

    /// <summary>
    /// Consulta o estado atual de conexões ativas no Hub.
    /// </summary>
    [HttpGet("metricas")]
    public IActionResult ObterMetricas()
    {
        return Ok(NotificacoesHub.ObterMetricas());
    }
}
