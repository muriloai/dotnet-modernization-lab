using Microsoft.EntityFrameworkCore;
using RepositoryPatternDemo.Data;
using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pedido?> ObterPorNumeroAsync(string numeroPedido, CancellationToken cancellationToken = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.NumeroPedido == numeroPedido, cancellationToken);
    }

    public async Task<IReadOnlyList<Pedido>> ObterPedidosRecentesAsync(int limite = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataCriacao)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CriarPedidoComTransacaoAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        // Transação atômica explícita e assíncrona gerenciada nativamente pelo EF Core
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            pedido.ValorTotal = pedido.Itens.Sum(i => i.Subtotal);
            pedido.DataCriacao = DateTime.UtcNow;

            await _context.Pedidos.AddAsync(pedido, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
