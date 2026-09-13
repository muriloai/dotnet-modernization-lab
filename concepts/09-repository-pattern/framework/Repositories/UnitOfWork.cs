using RepositoryPatternDemo.Data;
using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Repositories
{
    // Implementação clássica de Unit of Work que apenas repassa o SaveChanges do DbContext
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IRepository<Pedido> _pedidos;
        private IRepository<Cliente> _clientes;

        public UnitOfWork() : this(new AppDbContext())
        {
        }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<Pedido> Pedidos
        {
            get
            {
                return _pedidos ?? (_pedidos = new GenericRepository<Pedido>(_context));
            }
        }

        public IRepository<Cliente> Clientes
        {
            get
            {
                return _clientes ?? (_clientes = new GenericRepository<Cliente>(_context));
            }
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
