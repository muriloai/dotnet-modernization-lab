using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Application.Interfaces;

// Princípio da Inversão de Dependências (DIP) e Segregação de Interfaces (ISP)
public interface IClienteRepository
{
    Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task SalvarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ListarTodosAsync(CancellationToken cancellationToken = default);
}
