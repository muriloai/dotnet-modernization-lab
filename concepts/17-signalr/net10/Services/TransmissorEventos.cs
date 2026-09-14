using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Hubs;
using SignalRDemo.Models;

namespace SignalRDemo.Services;

/// <summary>
/// Implementação de serviço demonstrando o uso de IHubContext com contratos tipados.
/// Permite que controllers REST, filas assíncronas ou serviços em segundo plano (BackgroundServices)
/// publiquem eventos em tempo real sem precisar abrir conexões WebSockets cliente.
/// </summary>
public sealed class TransmissorEventos : ITransmissorEventos
{
    private readonly IHubContext<NotificacoesHub, INotificacaoCliente> _hubContext;

    public TransmissorEventos(IHubContext<NotificacoesHub, INotificacaoCliente> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task DispararNotificacaoExternaAsync(EnviarAvisoRequest request)
    {
        NotificacoesHub.IncrementarMensagens();

        var evento = new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: request.Titulo,
            Mensagem: request.Mensagem,
            Canal: string.IsNullOrWhiteSpace(request.Canal) ? "Geral (REST)" : request.Canal.Trim(),
            DataHoraUtc: DateTime.UtcNow
        );

        if (!string.IsNullOrWhiteSpace(request.Canal) && !string.Equals(request.Canal, "Global", StringComparison.OrdinalIgnoreCase))
        {
            await _hubContext.Clients.Group(request.Canal.Trim()).ReceberMensagemGrupo(request.Canal.Trim(), evento);
        }
        else
        {
            await _hubContext.Clients.All.ReceberNotificacao(evento);
        }

        await _hubContext.Clients.All.AtualizarMetricas(NotificacoesHub.ObterMetricas());
    }
}
