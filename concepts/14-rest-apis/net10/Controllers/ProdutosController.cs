using Microsoft.AspNetCore.Mvc;
using RestApisDemo.Models;
using RestApisDemo.Services;

namespace RestApisDemo.Controllers;

/// <summary>
/// Controller RESTful moderno no ASP.NET Core (.NET 10).
/// Demonstra o uso de [ApiController], ControllerBase, tipagem forte com ActionResult de T,
/// códigos de status semânticos (200, 201 com header Location, 204, 400, 404, 409)
/// e padronização RFC 7807 ProblemDetails.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    /// <summary>
    /// Retorna a lista de produtos com suporte a filtros opcionais por categoria e status de ativação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProdutoDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProdutoDto>> Listar([FromQuery] string? categoria, [FromQuery] bool? ativo)
    {
        var produtos = _produtoService.ListarTodos(categoria, ativo);
        return Ok(produtos);
    }

    /// <summary>
    /// Localiza um produto específico pelo seu identificador numérico.
    /// Retorna 200 OK com o objeto tipado ou 404 NotFound com ProblemDetails caso não exista.
    /// </summary>
    [HttpGet("{id:int}")]
    [ActionName(nameof(ObterPorId))]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ProdutoDto> ObterPorId(int id)
    {
        var produto = _produtoService.ObterPorId(id);
        if (produto is null)
        {
            return Problem(
                title: "Produto não encontrado",
                detail: $"Nenhum produto foi localizado com o identificador {id}.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(produto);
    }

    /// <summary>
    /// Cadastra um novo produto no catálogo.
    /// O atributo [ApiController] valida o modelo antes da execução do método.
    /// Em caso de sucesso, retorna 201 Created acompanhado do cabeçalho HTTP 'Location'.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<ProdutoDto> Criar([FromBody] CriarProdutoRequest request)
    {
        var (sucesso, produto, erro) = _produtoService.Criar(request);

        if (!sucesso)
        {
            return Problem(
                title: "Conflito de unicidade de SKU",
                detail: erro,
                statusCode: StatusCodes.Status409Conflict
            );
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = produto!.Id }, produto);
    }

    /// <summary>
    /// Substituição completa (PUT) dos dados de um produto existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ProdutoDto> Atualizar(int id, [FromBody] AtualizarProdutoRequest request)
    {
        var (sucesso, produto, erro) = _produtoService.Atualizar(id, request);

        if (!sucesso)
        {
            return Problem(
                title: "Falha na atualização",
                detail: erro,
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(produto);
    }

    /// <summary>
    /// Atualização parcial (PATCH) alterando exclusivamente o preço do produto.
    /// </summary>
    [HttpPatch("{id:int}/preco")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ProdutoDto> AtualizarPreco(int id, [FromBody] AtualizarPrecoRequest request)
    {
        var (sucesso, produto, erro) = _produtoService.AtualizarPreco(id, request.NovoPreco);

        if (!sucesso)
        {
            return Problem(
                title: "Falha na alteração de preço",
                detail: erro,
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(produto);
    }

    /// <summary>
    /// Remove um produto do catálogo. Retorna 204 No Content quando executado com êxito.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Excluir(int id)
    {
        var removido = _produtoService.Excluir(id);
        if (!removido)
        {
            return Problem(
                title: "Produto não encontrado",
                detail: $"Não foi possível excluir pois o produto com ID {id} não existe.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return NoContent();
    }
}
