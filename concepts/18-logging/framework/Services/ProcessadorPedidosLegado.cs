using System;
using System.Diagnostics;
using LoggingDemo.Models;

namespace LoggingDemo.Services
{
    public class ProcessadorPedidosLegado
    {
        public void Processar(PedidoLegado pedido, bool simularAlerta, bool simularErro)
        {
            // No legado, chamadas estaticas diretas sem DI e com string.Format / concatenacao
            Trace.TraceInformation(
                string.Format(
                    "Iniciando faturamento do pedido {0} para o cliente {1} ({2}) no valor de {3:C2} via canal {4}",
                    pedido.PedidoId,
                    pedido.ClienteId,
                    pedido.ClienteNome,
                    pedido.ValorTotal,
                    pedido.Canal));

            if (simularAlerta)
            {
                // Alerta via concatenacao simples, gerando alocacao no heap
                Trace.TraceWarning(
                    "Aviso de inventário: Saldo reduzido para itens do pedido " + pedido.PedidoId + ". Estoque restante: 2 unidades.");
            }

            if (simularErro)
            {
                try
                {
                    throw new InvalidOperationException("Falha na comunicação com o gateway financeiro legado: tempo limite esgotado.");
                }
                catch (Exception ex)
                {
                    // No legado, a exception era concatenada como texto plano
                    Trace.TraceError("Erro crítico ao processar o pedido " + pedido.PedidoId + ". Detalhes: " + ex.ToString());
                    return;
                }
            }

            Trace.TraceInformation("Pedido " + pedido.PedidoId + " faturado e concluído com sucesso.");
        }
    }
}
