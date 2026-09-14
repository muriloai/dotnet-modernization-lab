using System.Collections.Concurrent;
using PedidosCqrsDemo.Features.Pedidos.DTOs;

namespace PedidosCqrsDemo.Infrastructure.Persistence;

public class PedidoRepositoryEmMemoria
{
    private readonly ConcurrentDictionary<Guid, PedidoDetalheDto> _pedidos = new();

    public PedidoRepositoryEmMemoria()
    {
        // Mock inicial
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        _pedidos[id1] = new PedidoDetalheDto(id1, "TechCorp Brasil", "Servidor Dell PowerEdge", 15400.00m, 1, 15400.00m, "Faturado", DateTime.UtcNow.AddMinutes(-20));
        _pedidos[id2] = new PedidoDetalheDto(id2, "Logistica Alpha", "Leitor de Codigo de Barras Zebra", 850.00m, 3, 2550.00m, "Processando", DateTime.UtcNow.AddMinutes(-5));
    }

    public void Inserir(PedidoDetalheDto pedido)
    {
        _pedidos[pedido.Id] = pedido;
    }

    public IReadOnlyList<PedidoDetalheDto> ObterTodos()
    {
        return _pedidos.Values.OrderByDescending(p => p.CriadoEm).ToList();
    }

    public PedidoDetalheDto? ObterPorId(Guid id)
    {
        return _pedidos.TryGetValue(id, out var pedido) ? pedido : null;
    }
}
