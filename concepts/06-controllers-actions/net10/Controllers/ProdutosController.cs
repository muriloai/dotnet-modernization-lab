using ControllersAndActionsDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControllersAndActionsDemo.Controllers;

/// <summary>
/// Controller moderno de API herdando de ControllerBase.
/// O atributo [ApiController] habilita comportamentos de opinião do framework:
/// 1. Inferência automática de fontes de binding de parâmetros ([FromBody], [FromRoute], etc.).
/// 2. Validação automática do modelo: se o payload for inválido, o framework responde com
///    HTTP 400 ProblemDetails (RFC 7807) automaticamente, sem invocar a action.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private static readonly List<ProdutoDto> Produtos =
    [
        new(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
        new(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true),
        new(3, "Teclado Mecânico RGB", "perifericos", 350.00m, false)
    ];

    // GET: api/produtos
    [HttpGet]
    [ProducesResponseType<IEnumerable<ProdutoDto>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProdutoDto>> ObterTodos()
    {
        return Ok(Produtos);
    }

    // GET: api/produtos/1
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProdutoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProdutoDto> ObterPorId(int id)
    {
        var produto = Produtos.FirstOrDefault(p => p.Id == id);
        if (produto is null)
        {
            return NotFound(new { Mensagem = $"Produto com Id {id} não foi encontrado." });
        }

        return Ok(produto);
    }

    // POST: api/produtos
    // Observe: NÃO há if (!ModelState.IsValid)! O [ApiController] cuida da validação automaticamente.
    [HttpPost]
    [ProducesResponseType<ProdutoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProdutoDto> Criar(CriarProdutoRequest request)
    {
        var novoId = Produtos.Count > 0 ? Produtos.Max(p => p.Id) + 1 : 1;
        var novoProduto = new ProdutoDto(novoId, request.Nome, request.Categoria, request.Preco, request.EmEstoque);
        Produtos.Add(novoProduto);

        return CreatedAtAction(nameof(ObterPorId), new { id = novoProduto.Id }, novoProduto);
    }

    // DELETE: api/produtos/1
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Excluir(int id)
    {
        var produto = Produtos.FirstOrDefault(p => p.Id == id);
        if (produto is null)
        {
            return NotFound();
        }

        Produtos.Remove(produto);
        return NoContent();
    }
}
