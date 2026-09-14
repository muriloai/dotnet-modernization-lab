namespace SignalRDemo.Models;

/// <summary>
/// Modelo de entrada para disparo de notificações a partir de requisições REST externas.
/// </summary>
public sealed record EnviarAvisoRequest(
    string Titulo,
    string Mensagem,
    string? Canal
);
