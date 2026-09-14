using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolidArchitectureDemo.Application.Interfaces;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Infrastructure.Repositories;

// Princípio de Substituição de Liskov (LSP): substitui perfeitamente um repositório SQL
public class ClienteRepositoryEmMemoria : IClienteRepository
{
    private readonly ConcurrentDictionary<Guid, Cliente> _clientes = new();

    public Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var cliente = _clientes.Values.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(cliente);
    }

    public Task SalvarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _clientes[cliente.Id] = cliente;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Cliente>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cliente> lista = _clientes.Values.OrderByDescending(c => c.CriadoEm).ToList();
        return Task.FromResult(lista);
    }
}
