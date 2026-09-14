using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SignalRDemo.Startup))]

namespace SignalRDemo
{
    /// <summary>
    /// Ponto de entrada OWIN no .NET Framework 4.8.1.
    /// O ASP.NET SignalR 2.x exigia a infraestrutura Katana/OWIN para intermediar
    /// a conexão entre o servidor IIS e os WebSockets.
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Registra as rotas e manipuladores do SignalR legado (/signalr)
            app.MapSignalR();
        }
    }
}
