using Microsoft.EntityFrameworkCore;
using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        if (await context.Clientes.AnyAsync())
        {
            return;
        }

        var cliente1 = new Cliente { Nome = "Tech Solutions Ltda", Email = "compras@techsolutions.com.br", Documento = "12.345.678/0001-90" };
        var cliente2 = new Cliente { Nome = "Mariana Alves Santos", Email = "mariana.alves@email.com", Documento = "234.567.890-12" };

        await context.Clientes.AddRangeAsync(cliente1, cliente2);
        await context.SaveChangesAsync();

        var pedido1 = new Pedido
        {
            NumeroPedido = "PED-2026-001",
            ClienteId = cliente1.Id,
            DataCriacao = DateTime.UtcNow.AddDays(-3),
            Status = "Entregue",
            ValorTotal = 3450.00m,
            Itens =
            [
                new ItemPedido { DescricaoProduto = "Servidor Rack 1U", Quantidade = 1, PrecoUnitario = 3200.00m },
                new ItemPedido { DescricaoProduto = "Cabo de Rede Cat6 10m", Quantidade = 5, PrecoUnitario = 50.00m }
            ]
        };

        var pedido2 = new Pedido
        {
            NumeroPedido = "PED-2026-002",
            ClienteId = cliente2.Id,
            DataCriacao = DateTime.UtcNow.AddDays(-1),
            Status = "Processando",
            ValorTotal = 780.00m,
            Itens =
            [
                new ItemPedido { DescricaoProduto = "Monitor 24 polegadas IPS", Quantidade = 1, PrecoUnitario = 780.00m }
            ]
        };

        await context.Pedidos.AddRangeAsync(pedido1, pedido2);
        await context.SaveChangesAsync();
    }
}
