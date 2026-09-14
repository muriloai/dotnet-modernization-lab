namespace SignalRDemo.Models;

/// <summary>
/// Modelo de evento de notificação em tempo real transmitido pelo SignalR.
/// </summary>
public sealed record NotificacaoEvento(
    string Id,
    string Titulo,
    string Mensagem,
    string Canal,
    DateTime DataHoraUtc
);
