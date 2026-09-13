using System.Collections.Concurrent;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Services;

/// <summary>
/// Implementação em memória para demonstração de autorização sobre recursos específicos.
/// </summary>
public sealed class RelatorioService : IRelatorioService
{
    private readonly ConcurrentDictionary<int, RelatorioFinanceiro> _relatorios = new();

    public RelatorioService()
    {
        var rel1 = new RelatorioFinanceiro
        {
            Id = 1,
            Titulo = "Fechamento Mensal de Infraestrutura Cloud",
            Valor = 145000.00m,
            AutorEmail = "admin@empresa.com",
            AutorNome = "Ana Administradora",
            Departamento = "Tecnologia",
            Status = "Em Revisão",
            DataCriacaoUtc = DateTime.UtcNow.AddDays(-5)
        };

        var rel2 = new RelatorioFinanceiro
        {
            Id = 2,
            Titulo = "Auditoria Tributária e Balancete Q1",
            Valor = 89000.00m,
            AutorEmail = "gerente.financeiro@empresa.com",
            AutorNome = "Beatriz Gerente",
            Departamento = "Financeiro",
            Status = "Pendente",
            DataCriacaoUtc = DateTime.UtcNow.AddDays(-2)
        };

        var rel3 = new RelatorioFinanceiro
        {
            Id = 3,
            Titulo = "Reembolso de Diárias e Transporte",
            Valor = 2450.00m,
            AutorEmail = "analista.financeiro@empresa.com",
            AutorNome = "Claudio Analista",
            Departamento = "Financeiro",
            Status = "Aprovado",
            DataCriacaoUtc = DateTime.UtcNow.AddHours(-12)
        };

        _relatorios[rel1.Id] = rel1;
        _relatorios[rel2.Id] = rel2;
        _relatorios[rel3.Id] = rel3;
    }

    public Task<IEnumerable<RelatorioFinanceiro>> ListarTodosAsync()
    {
        return Task.FromResult<IEnumerable<RelatorioFinanceiro>>(_relatorios.Values.OrderBy(r => r.Id));
    }

    public Task<RelatorioFinanceiro?> ObterPorIdAsync(int id)
    {
        _relatorios.TryGetValue(id, out var relatorio);
        return Task.FromResult(relatorio);
    }

    public Task<bool> AtualizarStatusAsync(int id, string novoStatus)
    {
        if (_relatorios.TryGetValue(id, out var relatorio))
        {
            relatorio.Status = novoStatus;
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
