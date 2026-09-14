using Microsoft.Extensions.Logging;

namespace LoggingDemo.Services;

public static partial class LoggerOtimizado
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Processamento otimizado: Pedido {PedidoId} no valor de {ValorTotal} faturado com sucesso para {ClienteId}")]
    public static partial void LogPedidoFaturadoOtimizado(
        this ILogger logger,
        int pedidoId,
        decimal valorTotal,
        string clienteId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Alerta de estoque otimizado: Item do pedido {PedidoId} possui apenas {EstoqueRestante} unidades em saldo")]
    public static partial void LogAlertaEstoqueOtimizado(
        this ILogger logger,
        int pedidoId,
        int estoqueRestante);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Falha no faturamento otimizado: Erro ao debitar saldo para o pedido {PedidoId}")]
    public static partial void LogErroFaturamentoOtimizado(
        this ILogger logger,
        Exception ex,
        int pedidoId);
}
