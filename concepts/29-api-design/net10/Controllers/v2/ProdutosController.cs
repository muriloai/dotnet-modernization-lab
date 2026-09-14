using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ApiDesignDemo.Models;

namespace ApiDesignDemo.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private static readonly List<ProdutoV2Dto> _produtos = new()
    {
        new ProdutoV2Dto(
            Id: 1,
            Nome: "Monitor UltraWide 29 pol",
            Categoria: "Perifericos de Video",
            PrecoOriginal: 1299.90m,
            PercentualDesconto: 10.0m,
            PrecoFinal: 1169.91m,
            Links: new List<LinkHateoas>
            {
                new LinkHateoas("/api/v2/produtos/1", "self", "GET"),
                new LinkHateoas("/api/v2/produtos/1", "update", "PUT"),
                new LinkHateoas("/api/v2/produtos/1", "delete", "DELETE")
            }
        ),
        new ProdutoV2Dto(
            Id: 2,
            Nome: "Teclado Mecanico RGB",
            Categoria: "Acessorios Gamer",
            PrecoOriginal: 349.50m,
            PercentualDesconto: 15.0m,
            PrecoFinal: 297.07m,
            Links: new List<LinkHateoas>
            {
                new LinkHateoas("/api/v2/produtos/2", "self", "GET"),
                new LinkHateoas("/api/v2/produtos/2", "update", "PUT"),
                new LinkHateoas("/api/v2/produtos/2", "delete", "DELETE")
            }
        ),
        new ProdutoV2Dto(
            Id: 3,
            Nome: "Mouse Sem Fio Ergonomico",
            Categoria: "Ergonomia e Produtividade",
            PrecoOriginal: 189.00m,
            PercentualDesconto: 5.0m,
            PrecoFinal: 179.55m,
            Links: new List<LinkHateoas>
            {
                new LinkHateoas("/api/v2/produtos/3", "self", "GET"),
                new LinkHateoas("/api/v2/produtos/3", "update", "PUT"),
                new LinkHateoas("/api/v2/produtos/3", "delete", "DELETE")
            }
        )
    };

    [HttpGet]
    public IActionResult ObterTodos()
    {
        return Ok(new
        {
            VersaoApi = "2.0",
            TotalItens = _produtos.Count,
            Itens = _produtos,
            _links = new List<LinkHateoas>
            {
                new LinkHateoas("/api/v2/produtos", "self", "GET"),
                new LinkHateoas("/api/v2/produtos", "create", "POST")
            }
        });
    }

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == id);
        if (produto == null)
        {
            return Problem(
                detail: $"O item com ID {id} nao foi encontrado na versao 2.0 da API.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso Inexistente"
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
                detail: "O campo 'Nome' e mandatorio na V2 e requer ao menos 3 caracteres.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Violacao de Validacao V2"
            );
        }

        if (!request.Preco.HasValue || request.Preco.Value <= 0)
        {
            return Problem(
                detail: "O preco deve ser maior que zero.",
                instance: HttpContext.Request.Path,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Preco Invalido"
            );
        }

        var novoId = _produtos.Count > 0 ? _produtos.Max(p => p.Id) + 1 : 1;
        var categoria = string.IsNullOrWhiteSpace(request.Categoria) ? "Geral" : request.Categoria.Trim();
        var desconto = 10.0m;
        var precoFinal = request.Preco.Value * (1 - (desconto / 100));

        var novo = new ProdutoV2Dto(
            Id: novoId,
            Nome: request.Nome.Trim(),
            Categoria: categoria,
            PrecoOriginal: request.Preco.Value,
            PercentualDesconto: desconto,
            PrecoFinal: precoFinal,
            Links: new List<LinkHateoas>
            {
                new LinkHateoas($"/api/v2/produtos/{novoId}", "self", "GET"),
                new LinkHateoas($"/api/v2/produtos/{novoId}", "update", "PUT"),
                new LinkHateoas($"/api/v2/produtos/{novoId}", "delete", "DELETE")
            }
        );

        _produtos.Add(novo);
        return CreatedAtAction(nameof(ObterPorId), new { id = novo.Id, version = "2.0" }, novo);
    }
}
