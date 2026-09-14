using System;
using System.Collections.Generic;
using CSharpEvolutionDemo.Models;
using Microsoft.Extensions.Logging;

namespace CSharpEvolutionDemo.Services;

// Primary Constructor direto na declaração da classe (recurso C# 12+)
public class RegrasNegocioModernoService(ILogger<RegrasNegocioModernoService> logger)
{
    // Pattern Matching Avançado com Switch Expression e Padrões Relacionais/Propriedades
    public (decimal Desconto, string RegraAplicada) CalcularDesconto(Pedido pedido) => pedido switch
    {
        { Status: "Cancelado" } => (0m, "Pedido cancelado: nenhum desconto aplicável"),
        { Cliente.Categoria: "VIP", ValorTotal: >= 1000m } => (pedido.ValorTotal * 0.20m, "Cliente VIP com pedido >= R$ 1.000: 20% de desconto"),
        { Cliente.Categoria: "VIP", ValorTotal: >= 500m and < 1000m } => (pedido.ValorTotal * 0.15m, "Cliente VIP com pedido entre R$ 500 e R$ 999: 15% de desconto"),
        { Cliente.PontosFidelidade: >= 500, ValorTotal: >= 300m } => (pedido.ValorTotal * 0.10m, "Fidelidade Ouro (pontos >= 500) e pedido >= R$ 300: 10% de desconto"),
        { Itens: [_, _, ..] } and { ValorTotal: >= 200m } => (pedido.ValorTotal * 0.05m, "Carrinho volumoso (2 ou mais itens) e total >= R$ 200: 5% de desconto"),
        _ => (0m, "Regra padrão: sem desconto aplicável")
    };

    // Mutação não destrutiva de Record usando a expressão 'with'
    public Pedido AplicarDesconto(Pedido pedido)
    {
        var (desconto, regra) = CalcularDesconto(pedido);
        logger.LogInformation("Regra aplicada ao pedido {PedidoId}: {Regra}", pedido.Id, regra);

        // Cria uma nova instância imutável com valor recalculado sem modificar o original
        return pedido with { ValorTotal = Math.Max(0m, pedido.ValorTotal - desconto) };
    }

    // Collection Expressions e Raw String Literals (C# 11, 12, 13, 14)
    public string FormatarReciboJson(Pedido pedidoOriginal, Pedido pedidoComDesconto, string regra)
    {
        // Collection expression unificando itens existentes e brindes adicionais
        List<ItemPedido> itensTotais = [..pedidoOriginal.Itens, new ItemPedido("Cupom Digital", 1, 0.00m)];

        // Raw String Literal preservando aspas e indentação sem escape
        return $$"""
        {
            "pedidoId": {{pedidoOriginal.Id}},
            "cliente": {
                "nome": "{{pedidoOriginal.Cliente.Nome}}",
                "categoria": "{{pedidoOriginal.Cliente.Categoria}}",
                "pontos": {{pedidoOriginal.Cliente.PontosFidelidade}}
            },
            "valorOriginal": {{pedidoOriginal.ValorTotal.ToString("F2")}},
            "valorComDesconto": {{pedidoComDesconto.ValorTotal.ToString("F2")}},
            "regraAplicada": "{{regra}}",
            "totalItensComBrinde": {{itensTotais.Count}}
        }
        """;
    }
}
