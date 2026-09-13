using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestApisDemo.Models;
using RestApisDemo.Services;

namespace RestApisDemo.Endpoints;

/// <summary>
/// Mapeamento de rotas equivalentes utilizando Minimal APIs no .NET 10.
/// Demonstra como o modelo moderno permite definir contratos HTTP sem necessidade
/// de classes de controllers, utilizando TypedResults para tipagem em tempo de compilação.
/// </summary>
public static class ProdutosEndpoints
{
    public static RouteGroupBuilder MapProdutosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v2/produtos")
            .WithTags("Produtos (Minimal API)")
            .WithDescription("Endpoints de catálogo implementados via Minimal APIs no .NET 10");

        // GET /api/v2/produtos
        group.MapGet("/", Ok<IEnumerable<ProdutoDto>> (
            IProdutoService service,
            [FromQuery] string? categoria,
            [FromQuery] bool? ativo) =>
        {
            var produtos = service.ListarTodos(categoria, ativo);
            return TypedResults.Ok(produtos);
        })
        .WithName("ListarProdutosV2")
        .WithSummary("Lista produtos cadastrados (Minimal API)");

        // GET /api/v2/produtos/{id}
        group.MapGet("/{id:int}", Results<Ok<ProdutoDto>, NotFound<ProblemDetails>> (
            int id,
            IProdutoService service) =>
        {
            var produto = service.ObterPorId(id);
            return produto is not null
                ? TypedResults.Ok(produto)
                : TypedResults.NotFound(new ProblemDetails
                {
                    Title = "Produto não encontrado",
                    Detail = $"Nenhum produto foi localizado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
        })
        .WithName("ObterProdutoPorIdV2")
        .WithSummary("Obtém detalhes de um produto por ID (Minimal API)");

        // POST /api/v2/produtos
        group.MapPost("/", Results<Created<ProdutoDto>, Conflict<ProblemDetails>> (
            CriarProdutoRequest request,
            IProdutoService service) =>
        {
            var (sucesso, produto, erro) = service.Criar(request);
            if (!sucesso)
            {
                return TypedResults.Conflict(new ProblemDetails
                {
                    Title = "Conflito de unicidade de SKU",
                    Detail = erro,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return TypedResults.Created($"/api/v2/produtos/{produto!.Id}", produto);
        })
        .WithName("CriarProdutoV2")
        .WithSummary("Cadastra novo produto via Minimal API");

        // DELETE /api/v2/produtos/{id}
        group.MapDelete("/{id:int}", Results<NoContent, NotFound<ProblemDetails>> (
            int id,
            IProdutoService service) =>
        {
            var removido = service.Excluir(id);
            return removido
                ? TypedResults.NoContent()
                : TypedResults.NotFound(new ProblemDetails
                {
                    Title = "Produto não encontrado",
                    Detail = $"Não foi possível excluir o produto com ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
        })
        .WithName("ExcluirProdutoV2")
        .WithSummary("Remove produto do catálogo via Minimal API");

        return group;
    }
}
