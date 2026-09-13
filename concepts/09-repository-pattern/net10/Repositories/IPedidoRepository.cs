using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Repositories;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorNumeroAsync(string numeroPedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ObterPedidosRecentesAsync(int limite = 10, CancellationToken cancellationToken = default);
    Task<bool> CriarPedidoComTransacaoAsync(Pedido pedido, CancellationToken cancellationToken = default);
}
