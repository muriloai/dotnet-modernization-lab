using System;
using System.Collections.Generic;

namespace CSharpEvolutionDemo.Models
{
    public class ItemPedidoLegado
    {
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public ItemPedidoLegado()
        {
        }

        public ItemPedidoLegado(string produto, int quantidade, decimal precoUnitario)
        {
            Produto = produto;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
        }
    }

    public class PedidoLegado
    {
        public int Id { get; set; }
        public ClienteLegado Cliente { get; set; }
        public List<ItemPedidoLegado> Itens { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }

        public PedidoLegado()
        {
            Itens = new List<ItemPedidoLegado>();
        }

        public PedidoLegado(int id, ClienteLegado cliente, List<ItemPedidoLegado> itens, decimal valorTotal, string status)
        {
            Id = id;
            Cliente = cliente;
            Itens = itens ?? new List<ItemPedidoLegado>();
            ValorTotal = valorTotal;
            Status = status;
        }

        // Sem suporte a expressão 'with', clone manual era obrigatório
        public PedidoLegado ClonarComNovoValor(decimal novoValor)
        {
            var itensCopia = new List<ItemPedidoLegado>();
            foreach (var item in Itens)
            {
                itensCopia.Add(new ItemPedidoLegado(item.Produto, item.Quantidade, item.PrecoUnitario));
            }

            return new PedidoLegado(Id, Cliente, itensCopia, novoValor, Status);
        }
    }
}
