using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PedidosCqrsDemo.Models;

namespace PedidosCqrsDemo.Services
{
    // Servico monolitico implementando a Fat Interface com cross-cutting concerns repetidos em cada metodo
    public class PedidoMonoliticoService : IPedidoService
    {
        private static readonly ConcurrentDictionary<Guid, PedidoLegado> _bancoMemoria =
            new ConcurrentDictionary<Guid, PedidoLegado>();

        static PedidoMonoliticoService()
        {
            var p1 = new PedidoLegado
            {
                Cliente = "TechCorp Brasil",
                Produto = "Servidor Dell PowerEdge",
                ValorUnitario = 15400.00m,
                Quantidade = 1,
                ValorTotal = 15400.00m,
                Status = "Faturado",
                DataCriacao = DateTime.Now.AddMinutes(-20)
            };
            var p2 = new PedidoLegado
            {
                Cliente = "Logistica Alpha",
                Produto = "Leitor Zebra",
                ValorUnitario = 850.00m,
                Quantidade = 3,
                ValorTotal = 2550.00m,
                Status = "Processando",
                DataCriacao = DateTime.Now.AddMinutes(-5)
            };
            _bancoMemoria[p1.Id] = p1;
            _bancoMemoria[p2.Id] = p2;
        }

        public void InserirPedido(PedidoLegado pedido)
        {
            // Cross-cutting repetido manualmente
            var sw = Stopwatch.StartNew();
            Debug.WriteLine($"[LEGADO] Iniciando InserirPedido para {pedido.Cliente}");

            if (string.IsNullOrWhiteSpace(pedido.Cliente))
                throw new ArgumentException("Cliente obrigatorio");

            pedido.ValorTotal = pedido.ValorUnitario * pedido.Quantidade;
            _bancoMemoria[pedido.Id] = pedido;

            sw.Stop();
            Debug.WriteLine($"[LEGADO] InserirPedido concluido em {sw.ElapsedMilliseconds} ms");
        }

        public PedidoLegado ObterPorId(Guid id)
        {
            _bancoMemoria.TryGetValue(id, out var p);
            return p;
        }

        public List<PedidoLegado> ObterTodos()
        {
            var sw = Stopwatch.StartNew();
            Debug.WriteLine("[LEGADO] Iniciando ObterTodos");

            var lista = _bancoMemoria.Values.OrderByDescending(p => p.DataCriacao).ToList();

            sw.Stop();
            Debug.WriteLine($"[LEGADO] ObterTodos concluido em {sw.ElapsedMilliseconds} ms");
            return lista;
        }

        public void AtualizarStatus(Guid id, string novoStatus)
        {
            if (_bancoMemoria.TryGetValue(id, out var p))
            {
                p.Status = novoStatus;
            }
        }

        public void CancelarPedido(Guid id, string motivo)
        {
            if (_bancoMemoria.TryGetValue(id, out var p))
            {
                p.Status = "Cancelado";
                p.ObservacoesInternas = "Motivo: " + motivo;
            }
        }

        public decimal CalcularFrete(string cep, decimal peso)
        {
            return 25.00m + (peso * 1.5m);
        }

        public List<PedidoLegado> ObterRelatorioMensal(int mes, int ano)
        {
            return _bancoMemoria.Values
                .Where(p => p.DataCriacao.Month == mes && p.DataCriacao.Year == ano)
                .ToList();
        }
    }
}
