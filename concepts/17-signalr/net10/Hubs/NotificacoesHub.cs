using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Models;

namespace SignalRDemo.Hubs;

/// <summary>
/// Hub SignalR fortemente tipado no .NET 10.
/// Herda de Hub de INotificacaoCliente, garantindo segurança de tipos nas chamadas enviadas aos clientes.
/// Suporta gerenciamento de grupos de canais e atualização contínua de métricas de conexões ativas.
/// </summary>
public sealed class NotificacoesHub : Hub<INotificacaoCliente>
{
    private static int _conexoesAtivas;
    private static int _totalMensagens;
    private static readonly Lock _lock = new();

    public static MetricasTransmissao ObterMetricas()
    {
        lock (_lock)
        {
            return new MetricasTransmissao(_conexoesAtivas, _totalMensagens, DateTime.UtcNow);
        }
    }

    public static void IncrementarMensagens()
    {
        lock (_lock)
        {
            _totalMensagens++;
        }
    }

    public override async Task OnConnectedAsync()
    {
        lock (_lock)
        {
            _conexoesAtivas++;
        }

        await base.OnConnectedAsync();
        await Clients.All.AtualizarMetricas(ObterMetricas());

        // Envia notificação de boas-vindas exclusiva para o cliente que acabou de se conectar
        await Clients.Caller.ReceberNotificacao(new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: "Conexão Estabelecida",
            Mensagem: $"Conectado com sucesso ao Hub moderno no .NET 10. ConnectionId: {Context.ConnectionId}",
            Canal: "Sistema",
            DataHoraUtc: DateTime.UtcNow
        ));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            _conexoesAtivas = Math.Max(0, _conexoesAtivas - 1);
        }

        await base.OnDisconnectedAsync(exception);
        await Clients.All.AtualizarMetricas(ObterMetricas());
    }

    /// <summary>
    /// Envia uma notificação global para todos os clientes conectados.
    /// </summary>
    public async Task TransmitirAvisoGlobal(string titulo, string mensagem)
    {
        IncrementarMensagens();

        var evento = new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: titulo,
            Mensagem: mensagem,
            Canal: "Global",
            DataHoraUtc: DateTime.UtcNow
        );

        await Clients.All.ReceberNotificacao(evento);
        await Clients.All.AtualizarMetricas(ObterMetricas());
    }

    /// <summary>
    /// Inscreve a conexão atual em um grupo específico de canal.
    /// </summary>
    public async Task EntrarNoCanal(string canal)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, canal);

        await Clients.Caller.ReceberNotificacao(new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: "Inscrição em Canal",
            Mensagem: $"Você agora faz parte do canal '{canal}'.",
            Canal: canal,
            DataHoraUtc: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Desinscreve a conexão atual de um grupo de canal.
    /// </summary>
    public async Task SairDoCanal(string canal)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, canal);

        await Clients.Caller.ReceberNotificacao(new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: "Saída de Canal",
            Mensagem: $"Você saiu do canal '{canal}'.",
            Canal: canal,
            DataHoraUtc: DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Envia mensagem direcionada exclusivamente aos membros inscritos em um canal.
    /// </summary>
    public async Task TransmitirParaCanal(string canal, string titulo, string mensagem)
    {
        IncrementarMensagens();

        var evento = new NotificacaoEvento(
            Id: Guid.NewGuid().ToString("N")[..8],
            Titulo: titulo,
            Mensagem: mensagem,
            Canal: canal,
            DataHoraUtc: DateTime.UtcNow
        );

        await Clients.Group(canal).ReceberMensagemGrupo(canal, evento);
        await Clients.All.AtualizarMetricas(ObterMetricas());
    }
}
