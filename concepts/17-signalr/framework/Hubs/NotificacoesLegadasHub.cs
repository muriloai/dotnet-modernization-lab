using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalRDemo.Hubs
{
    /// <summary>
    /// Hub do ASP.NET SignalR 2.x no .NET Framework 4.8.1.
    /// Herda de Microsoft.AspNet.SignalR.Hub sem tipagem estrita no cliente.
    /// As chamadas aos clientes dependem de 'Clients.All.nomeDoMetodo(...)',
    /// onde o compilador C# não valida a existência do método em tempo de compilação (dynamic).
    /// </summary>
    public class NotificacoesLegadasHub : Hub
    {
        private static int _conexoesAtivas;
        private static int _totalMensagens;
        private static readonly object _lock = new object();

        public override Task OnConnected()
        {
            lock (_lock)
            {
                _conexoesAtivas++;
            }

            // Notifica todos os clientes passando argumentos dinâmicos
            Clients.All.atualizarMetricas(_conexoesAtivas, _totalMensagens);

            Clients.Caller.receberNotificacao(
                "SYS-" + Guid.NewGuid().ToString().Substring(0, 6),
                "Conexão Legada Estabelecida",
                "Conectado ao Hub clássico via ASP.NET SignalR 2.x. ConnectionId: " + Context.ConnectionId,
                "Sistema",
                DateTime.Now.ToString("HH:mm:ss")
            );

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            lock (_lock)
            {
                _conexoesAtivas = Math.Max(0, _conexoesAtivas - 1);
            }

            Clients.All.atualizarMetricas(_conexoesAtivas, _totalMensagens);
            return base.OnDisconnected(stopCalled);
        }

        public void TransmitirAvisoGlobal(string titulo, string mensagem)
        {
            lock (_lock)
            {
                _totalMensagens++;
            }

            var id = "MSG-" + Guid.NewGuid().ToString().Substring(0, 6);

            // Chamada com objeto dynamic: sem verificação estrita pelo compilador
            Clients.All.receberNotificacao(
                id,
                titulo,
                mensagem,
                "Global",
                DateTime.Now.ToString("HH:mm:ss")
            );

            Clients.All.atualizarMetricas(_conexoesAtivas, _totalMensagens);
        }

        public void EntrarNoCanal(string canal)
        {
            Groups.Add(Context.ConnectionId, canal);

            Clients.Caller.receberNotificacao(
                "GRP-" + Guid.NewGuid().ToString().Substring(0, 6),
                "Inscrição em Canal Legado",
                "Você foi adicionado ao grupo '" + canal + "' via Groups.Add().",
                canal,
                DateTime.Now.ToString("HH:mm:ss")
            );
        }

        public void SairDoCanal(string canal)
        {
            Groups.Remove(Context.ConnectionId, canal);

            Clients.Caller.receberNotificacao(
                "GRP-" + Guid.NewGuid().ToString().Substring(0, 6),
                "Saída de Canal Legado",
                "Você saiu do grupo '" + canal + "'.",
                canal,
                DateTime.Now.ToString("HH:mm:ss")
            );
        }

        public void TransmitirParaCanal(string canal, string titulo, string mensagem)
        {
            lock (_lock)
            {
                _totalMensagens++;
            }

            var id = "MSG-" + Guid.NewGuid().ToString().Substring(0, 6);

            Clients.Group(canal).receberMensagemGrupo(
                canal,
                id,
                titulo,
                mensagem,
                DateTime.Now.ToString("HH:mm:ss")
            );

            Clients.All.atualizarMetricas(_conexoesAtivas, _totalMensagens);
        }
    }
}
