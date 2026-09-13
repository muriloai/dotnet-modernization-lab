using System;
using System.Collections.Generic;
using System.Data.Entity;
using RepositoryPatternDemo.Models;

namespace RepositoryPatternDemo.Data
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            var cliente1 = new Cliente { Nome = "Tech Solutions Ltda", Email = "compras@techsolutions.com.br", Documento = "12.345.678/0001-90" };
            var cliente2 = new Cliente { Nome = "Mariana Alves Santos", Email = "mariana.alves@email.com", Documento = "234.567.890-12" };

            context.Clientes.Add(cliente1);
            context.Clientes.Add(cliente2);
            context.SaveChanges();

            var pedido1 = new Pedido
            {
                NumeroPedido = "PED-2026-001",
                ClienteId = cliente1.Id,
                DataCriacao = DateTime.UtcNow.AddDays(-3),
                Status = "Entregue",
                ValorTotal = 3450.00m,
                Itens = new List<ItemPedido>
                {
                    new ItemPedido { DescricaoProduto = "Servidor Rack 1U", Quantidade = 1, PrecoUnitario = 3200.00m },
                    new ItemPedido { DescricaoProduto = "Cabo de Rede Cat6 10m", Quantidade = 5, PrecoUnitario = 50.00m }
                }
            };

            var pedido2 = new Pedido
            {
                NumeroPedido = "PED-2026-002",
                ClienteId = cliente2.Id,
                DataCriacao = DateTime.UtcNow.AddDays(-1),
                Status = "Processando",
                ValorTotal = 780.00m,
                Itens = new List<ItemPedido>
                {
                    new ItemPedido { DescricaoProduto = "Monitor 24 polegadas IPS", Quantidade = 1, PrecoUnitario = 780.00m }
                }
            };

            context.Pedidos.Add(pedido1);
            context.Pedidos.Add(pedido2);
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
