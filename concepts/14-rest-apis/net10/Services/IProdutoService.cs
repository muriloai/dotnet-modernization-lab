using RestApisDemo.Models;

namespace RestApisDemo.Services;

/// <summary>
/// Contrato de serviço para operações de catálogo de produtos.
/// Injetado via DI nativa do ASP.NET Core tanto em Controllers quanto em Minimal APIs.
/// </summary>
public interface IProdutoService
{
    IEnumerable<ProdutoDto> ListarTodos(string? categoria = null, bool? apenasAtivos = null);
    ProdutoDto? ObterPorId(int id);
    ProdutoDto? ObterPorSku(string sku);
    (bool Sucesso, ProdutoDto? Produto, string? Erro) Criar(CriarProdutoRequest request);
    (bool Sucesso, ProdutoDto? Produto, string? Erro) Atualizar(int id, AtualizarProdutoRequest request);
    (bool Sucesso, ProdutoDto? Produto, string? Erro) AtualizarPreco(int id, decimal novoPreco);
    bool Excluir(int id);
}
