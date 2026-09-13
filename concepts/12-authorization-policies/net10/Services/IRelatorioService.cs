using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Services;

/// <summary>
/// Contrato de persistência e consulta de relatórios financeiros.
/// </summary>
public interface IRelatorioService
{
    Task<IEnumerable<RelatorioFinanceiro>> ListarTodosAsync();
    Task<RelatorioFinanceiro?> ObterPorIdAsync(int id);
    Task<bool> AtualizarStatusAsync(int id, string novoStatus);
}
