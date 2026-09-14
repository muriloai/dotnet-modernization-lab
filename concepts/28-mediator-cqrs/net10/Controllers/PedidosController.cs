using Microsoft.AspNetCore.Mvc;
using PedidosCqrsDemo.Features.Pedidos.Commands;
using PedidosCqrsDemo.Features.Pedidos.DTOs;
using PedidosCqrsDemo.Features.Pedidos.Queries;
using PedidosCqrsDemo.Infrastructure.Mediator;

namespace PedidosCqrsDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TraceLogService _traceLog;

    public PedidosController(IMediator mediator, TraceLogService traceLog)
    {
        _mediator = mediator;
        _traceLog = traceLog;
    }

    [HttpGet]
    public async Task<IActionResult> ObterPedidos(CancellationToken cancellationToken)
    {
        var pedidos = await _mediator.Send(new ObterPedidosQuery(), cancellationToken);
        return Ok(pedidos);
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Cliente) || string.IsNullOrWhiteSpace(request.Produto))
        {
            return BadRequest(new { Mensagem = "Cliente e produto sao obrigatorios." });
        }

        if (request.ValorUnitario <= 0 || request.Quantidade <= 0)
        {
            return BadRequest(new { Mensagem = "Valor unitario e quantidade devem ser maiores que zero." });
        }

        var command = new CriarPedidoCommand(request.Cliente, request.Produto, request.ValorUnitario, request.Quantidade);
        var resultado = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(ObterPedidos), new { id = resultado.Id }, resultado);
    }

    [HttpGet("traces")]
    public IActionResult ObterTraces()
    {
        return Ok(_traceLog.ObterTraces());
    }
}
