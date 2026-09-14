using System.Collections.Generic;
using CSharpEvolutionDemo.Models;
using CSharpEvolutionDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEvolutionDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CSharpEvolutionController(RegrasNegocioModernoService service) : ControllerBase
{
    public record RequisicaoAvaliacao(
        string NomeCliente,
        string CategoriaCliente,
        int PontosFidelidade,
        decimal ValorTotal,
        int QuantidadeItens,
        string Status
    );

    [HttpPost("avaliar-pedido")]
    public IActionResult AvaliarPedido([FromBody] RequisicaoAvaliacao req)
    {
        var cliente = new Cliente(1, req.NomeCliente, req.CategoriaCliente, req.PontosFidelidade);

        // Cria lista usando collection expressions
        List<ItemPedido> itens = [];
        for (int i = 1; i <= req.QuantidadeItens; i++)
        {
            itens.Add(new ItemPedido($"Item #{i}", 1, req.ValorTotal / (req.QuantidadeItens > 0 ? req.QuantidadeItens : 1)));
        }

        var pedidoOriginal = new Pedido(1001, cliente, itens, req.ValorTotal, req.Status);
        var (desconto, regra) = service.CalcularDesconto(pedidoOriginal);
        var pedidoComDesconto = service.AplicarDesconto(pedidoOriginal);
        var reciboJson = service.FormatarReciboJson(pedidoOriginal, pedidoComDesconto, regra);

        return Ok(new
        {
            PedidoOriginal = pedidoOriginal,
            PedidoComDesconto = pedidoComDesconto,
            DescontoConcedido = desconto,
            RegraAplicada = regra,
            ReciboGeradoViaRawString = reciboJson
        });
    }
}
