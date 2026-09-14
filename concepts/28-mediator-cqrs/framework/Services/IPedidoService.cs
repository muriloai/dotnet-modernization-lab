using System;
using System.Collections.Generic;
using PedidosCqrsDemo.Models;

namespace PedidosCqrsDemo.Services
{
    // Anti-padrao: Fat Interface que mistura criacao, faturamento, calculos de frete, relatorios e notificacoes
    public interface IPedidoService
    {
        void InserirPedido(PedidoLegado pedido);
        PedidoLegado ObterPorId(Guid id);
        List<PedidoLegado> ObterTodos();
        void AtualizarStatus(Guid id, string novoStatus);
        void CancelarPedido(Guid id, string motivo);
        decimal CalcularFrete(string cep, decimal peso);
        List<PedidoLegado> ObterRelatorioMensal(int mes, int ano);
    }
}
