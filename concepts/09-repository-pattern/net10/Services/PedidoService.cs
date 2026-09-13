using Microsoft.EntityFrameworkCore;
using RepositoryPatternDemo.Data;
using RepositoryPatternDemo.Models;
using RepositoryPatternDemo.Models.DTOs;

namespace RepositoryPatternDemo.Services;

public class PedidoService
{
    private readonly AppDbContext _context;

    public PedidoService(AppDbContext context)
    {
        _context = context;
    }

    // Abordagem Moderna Direta: O DbContext é injetado diretamente no serviço.
    // Projeção eficiente direto para o record DTO no SELECT do SQL,
    // evitando transportar entidades inteiras para a memória RAM.
    public async Task<IReadOnlyList<PedidoResumoDto>> ListarResumosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .OrderByDescending(p => p.DataCriacao)
            .Select(p => new PedidoResumoDto(
                p.Id,
                p.NumeroPedido,
                p.Cliente!.Nome,
                p.DataCriacao,
                p.Status,
                p.ValorTotal,
                p.Itens.Count
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ObterClientesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }
}
