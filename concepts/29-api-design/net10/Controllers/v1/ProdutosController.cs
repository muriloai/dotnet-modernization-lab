using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ApiDesignDemo.Models;

namespace ApiDesignDemo.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private static readonly List<ProdutoV1Dto> _produtos = new()
    {
        new ProdutoV1Dto(1, "Monitor UltraWide 29 pol", 1299.90m),
        new ProdutoV1Dto(2, "Teclado Mecanico RGB", 349.50m),
        new ProdutoV1Dto(3, "Mouse Sem Fio Ergonomico", 189.00m)
    };

    [HttpGet]
    public IActionResult ObterTodos()
    {
        return Ok(_produtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == id);
        if (produto == null)
        {
            return Problem(
                detail: $"Produto com identificador {id} nao foi localizado no catalogo.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status404NotFound,
                title: "Produto Nao Encontrado"
            );
        }
        return Ok(produto);
    }

    [HttpPost]
    public IActionResult Criar([FromBody] CriarProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Problem(
                detail: "O campo 'Nome' e obrigatorio e nao pode ser vazio.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Falha de Validacao de Contrato"
            );
        }

        if (!request.Preco.HasValue || request.Preco.Value <= 0)
        {
            return Problem(
                detail: "O preco do produto deve ser estritamente positivo.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Valor Invalido"
            );
        }

        var novoId = _produtos.Count > 0 ? _produtos.Max(p => p.Id) + 1 : 1;
        var novo = new ProdutoV1Dto(novoId, request.Nome.Trim(), request.Preco.Value);
        _produtos.Add(novo);

        return CreatedAtAction(nameof(ObterPorId), new { id = novo.Id, version = "1.0" }, novo);
    }
}
