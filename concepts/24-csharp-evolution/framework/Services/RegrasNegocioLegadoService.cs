using System;
using System.Collections.Generic;
using CSharpEvolutionDemo.Models;

namespace CSharpEvolutionDemo.Services
{
    public class RegrasNegocioLegadoService
    {
        public void CalcularDesconto(PedidoLegado pedido, out decimal desconto, out string regra)
        {
            desconto = 0m;
            regra = "Regra padrão: sem desconto aplicável";

            // Validações defensivas manuais contra nulos obrigatórias no C# 7.3
            if (pedido == null)
            {
                regra = "Pedido nulo";
                return;
            }

            if (string.Equals(pedido.Status, "Cancelado", StringComparison.OrdinalIgnoreCase))
            {
                desconto = 0m;
                regra = "Pedido cancelado: nenhum desconto aplicável";
                return;
            }

            if (pedido.Cliente != null && string.Equals(pedido.Cliente.Categoria, "VIP", StringComparison.OrdinalIgnoreCase))
            {
                if (pedido.ValorTotal >= 1000m)
                {
                    desconto = pedido.ValorTotal * 0.20m;
                    regra = "Cliente VIP com pedido >= R$ 1.000: 20% de desconto";
                    return;
                }
                else if (pedido.ValorTotal >= 500m)
                {
                    desconto = pedido.ValorTotal * 0.15m;
                    regra = "Cliente VIP com pedido entre R$ 500 e R$ 999: 15% de desconto";
                    return;
                }
            }

            if (pedido.Cliente != null && pedido.Cliente.PontosFidelidade >= 500 && pedido.ValorTotal >= 300m)
            {
                desconto = pedido.ValorTotal * 0.10m;
                regra = "Fidelidade Ouro (pontos >= 500) e pedido >= R$ 300: 10% de desconto";
                return;
            }

            if (pedido.Itens != null && pedido.Itens.Count >= 2 && pedido.ValorTotal >= 200m)
            {
                desconto = pedido.ValorTotal * 0.05m;
                regra = "Carrinho volumoso (2 ou mais itens) e total >= R$ 200: 5% de desconto";
                return;
            }
        }

        public string FormatarReciboLegado(PedidoLegado pedidoOriginal, PedidoLegado pedidoComDesconto, string regra)
        {
            // Concatenação de JSON com aspas escapadas manualmente em C# 7.3
            return string.Format(
                "{{\n" +
                "    \"pedidoId\": {0},\n" +
                "    \"cliente\": {{\n" +
                "        \"nome\": \"{1}\",\n" +
                "        \"categoria\": \"{2}\",\n" +
                "        \"pontos\": {3}\n" +
                "    }},\n" +
                "    \"valorOriginal\": {4:F2},\n" +
                "    \"valorComDesconto\": {5:F2},\n" +
                "    \"regraAplicada\": \"{6}\"\n" +
                "}}",
                pedidoOriginal.Id,
                pedidoOriginal.Cliente != null ? pedidoOriginal.Cliente.Nome : "Desconhecido",
                pedidoOriginal.Cliente != null ? pedidoOriginal.Cliente.Categoria : "Nenhum",
                pedidoOriginal.Cliente != null ? pedidoOriginal.Cliente.PontosFidelidade : 0,
                pedidoOriginal.ValorTotal,
                pedidoComDesconto.ValorTotal,
                regra
            );
        }
    }
}
