using System;
using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Repositories
{
    // Interface clássica de Unit of Work para coordenar múltiplos repositórios
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Pedido> Pedidos { get; }
        IRepository<Cliente> Clientes { get; }
        int Complete();
    }
}
