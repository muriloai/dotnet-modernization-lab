using ErrorHandlingDemo.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ErrorHandlingDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    public record CompraProdutoRequest(int ProdutoId, int Quantidade, bool ClienteVip);

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        if (id == 999)
        {
            throw new EntidadeNaoEncontradaException("Produto", id);
        }

        return Ok(new
        {
            id,
            nome = id == 1 ? "Notebook Corporativo Ultra" : $"Produto #{id}",
            preco = 4500.00m,
            estoqueDisponivel = 15,
            ativo = true
        });
    }

    [HttpPost("comprar")]
    public IActionResult Comprar([FromBody] CompraProdutoRequest requisicao)
    {
        if (requisicao.Quantidade <= 0)
        {
            throw new RegraNegocioException("QUANTIDADE_INVALIDA", "A quantidade de compra deve ser estritamente maior que zero.");
        }

        if (requisicao.Quantidade > 10 && !requisicao.ClienteVip)
        {
            throw new RegraNegocioException(
                "LIMITE_COMPRA_EXCEDIDO",
                "Clientes sem categoria VIP possuem um limite máximo de 10 unidades por compra.");
        }

        if (requisicao.ProdutoId == 500)
        {
            throw new IntegracaoExternaException(
                "GatewayAntiFraude",
                "O serviço de validação cadastral e antifraude retornou tempo limite esgotado (Timeout).");
        }

        if (requisicao.ProdutoId == 666)
        {
            throw new NullReferenceException("Referência de ponteiro nula simulada para validação de fallback genérico.");
        }

        var valorUnitario = requisicao.ProdutoId == 1 ? 4500.00m : 250.00m;
        var valorTotal = requisicao.Quantidade * valorUnitario;

        return Ok(new
        {
            sucesso = true,
            mensagem = "Pedido faturado com sucesso!",
            produtoId = requisicao.ProdutoId,
            quantidade = requisicao.Quantidade,
            valorTotal,
            dataHoraProcessamento = DateTime.UtcNow
        });
    }
}
