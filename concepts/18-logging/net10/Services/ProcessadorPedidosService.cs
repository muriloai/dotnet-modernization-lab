using LoggingDemo.Models;

namespace LoggingDemo.Services;

public class ProcessadorPedidosService
{
    private readonly ILogger<ProcessadorPedidosService> _logger;

    public ProcessadorPedidosService(ILogger<ProcessadorPedidosService> logger)
    {
        _logger = logger;
    }

    public void Processar(RequisicaoProcessamento requisicao, string correlationId)
    {
        // Criacao de escopo enriquecido com variaveis de correlacao e contexto do usuario
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["CanalOrigem"] = requisicao.Canal,
            ["Operador"] = "sistema.integracao"
        }))
        {
            if (requisicao.UsarLoggerOtimizado)
            {
                ExecutarFluxoOtimizado(requisicao);
            }
            else
            {
                ExecutarFluxoEstruturadoPadrao(requisicao);
            }
        }
    }

    private void ExecutarFluxoEstruturadoPadrao(RequisicaoProcessamento req)
    {
        _logger.LogInformation(
            "Iniciando faturamento do pedido {PedidoId} para o cliente {ClienteId} ({ClienteNome}) com {QuantidadeItens} itens no valor de {ValorTotal}",
            req.PedidoId,
            req.ClienteId,
            req.ClienteNome,
            req.QuantidadeItens,
            req.ValorTotal);

        if (req.SimularAlertaEstoque)
        {
            _logger.LogWarning(
                "Nível reduzido de estoque detectado para o produto do pedido {PedidoId}. Saldo em armazém: {SaldoEstoque}",
                req.PedidoId,
                3);
        }

        if (req.SimularErro)
        {
            try
            {
                throw new InvalidOperationException("Falha na comunicação com o gateway financeiro: tempo limite esgotado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha irrecuperável durante o faturamento do pedido {PedidoId} no total de {ValorTotal}",
                    req.PedidoId,
                    req.ValorTotal);
                return;
            }
        }

        _logger.LogInformation(
            "Pedido {PedidoId} aprovado e faturado com sucesso. Código de rastreio: {CodigoRastreio}",
            req.PedidoId,
            $"BR-{req.PedidoId:D6}-EXP");
    }

    private void ExecutarFluxoOtimizado(RequisicaoProcessamento req)
    {
        if (req.SimularAlertaEstoque)
        {
            _logger.LogAlertaEstoqueOtimizado(req.PedidoId, 2);
        }

        if (req.SimularErro)
        {
            try
            {
                throw new TimeoutException("Tempo de conexão excedido no barramento de mensageria.");
            }
            catch (Exception ex)
            {
                _logger.LogErroFaturamentoOtimizado(ex, req.PedidoId);
                return;
            }
        }

        _logger.LogPedidoFaturadoOtimizado(req.PedidoId, req.ValorTotal, req.ClienteId);
    }
}
