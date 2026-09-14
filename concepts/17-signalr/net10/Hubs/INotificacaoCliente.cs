using SignalRDemo.Models;

namespace SignalRDemo.Hubs;

/// <summary>
/// Contrato de interface fortemente tipado para os clientes SignalR no .NET 10.
/// Elimina chamadas dinâmicas (dynamic) do legado, garantindo validação em tempo de compilação
/// e suporte completo a refatorações automáticas pela IDE.
/// </summary>
public interface INotificacaoCliente
{
    Task ReceberNotificacao(NotificacaoEvento evento);
    Task ReceberMensagemGrupo(string grupo, NotificacaoEvento evento);
    Task AtualizarMetricas(MetricasTransmissao metricas);
}
