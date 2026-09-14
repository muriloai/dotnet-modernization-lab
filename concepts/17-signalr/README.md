# Conceito 17: Comunicação em Tempo Real com SignalR

Este laboratório compara o desenvolvimento de aplicações em tempo real com **ASP.NET SignalR 2.x no .NET Framework 4.8.1** e a versão totalmente reescrita no **ASP.NET Core SignalR no .NET 10**.

---

## 1. Visão Geral e a Grande Ruptura Arquitetural

O ASP.NET SignalR foi lançado originalmente para simplificar o envio de notificações push do servidor para clientes conectados através de WebSockets, Server-Sent Events ou Long Polling.

No entanto, a versão original para .NET Framework acumulou dívidas técnicas severas:
- **Dependência mandatória de jQuery:** O cliente JavaScript exigia o carregamento de `jquery.js` e do script gerado dinamicamente pelo servidor em `/signalr/hubs`.
- **Uso excessivo de tipagem dinâmica (`dynamic`):** No servidor, o envio de mensagens utilizava chamadas como `Clients.All.notificarMensagem(payload)`. Como `Clients.All` era um objeto `dynamic`, erros de digitação no nome do evento passavam despercebidos na compilação.
- **Acoplamento com OWIN e IIS:** Exigia o pipeline Katana/OWIN configurado sobre o `System.Web`.

No .NET 10, o ASP.NET Core SignalR foi reescrito a partir do zero:
- **Cliente JavaScript moderno e independente:** Biblioteca oficial `@microsoft/signalr` em TypeScript/JavaScript puro (ES6), sem qualquer dependência de jQuery.
- **Hubs Fortemente Tipados (`Hub<T>`):** Definição de interfaces de contrato para os clientes (ex: `INotificacaoCliente`), permitindo que a IDE valide nomes de métodos e parâmetros em tempo de compilação.
- **Injeção de Dependências e IHubContext:** Qualquer controller, Minimal API ou BackgroundService pode injetar `IHubContext<NotificacoesHub, INotificacaoCliente>` para disparar eventos para clientes conectados sem precisar abrir conexões extras.
- **MessagePack e Alto Desempenho:** Suporte a protocolo binário MessagePack que reduz drasticamente o tráfego de rede e o uso de memória sob alta concorrência.
- **Streaming Bidirecional:** Suporte nativo a `IAsyncEnumerable<T>` para envio contínuo de dados em tempo real.

---

## 2. Comparativo Técnico: Legado vs Moderno

| Característica | ASP.NET SignalR 2.x (.NET Framework 4.8.1) | ASP.NET Core SignalR (.NET 10) |
| :--- | :--- | :--- |
| **Dependência do Cliente Web** | Exigia jQuery e `jquery.signalR.js` | JavaScript puro moderno (`@microsoft/signalr`), zero dependências |
| **Geração de Proxies** | Script dinâmico em `/signalr/hubs` | Conexões explícitas sem código mágico gerado |
| **Tipagem do Hub** | Dinâmica via `Clients.All.metodo(...)` (`dynamic`) | Fortemente tipada via `Hub<TClient>` e interfaces C# |
| **Injeção Externa** | `GlobalHost.ConnectionManager` estático | Injeção nativa de `IHubContext<THub, TClient>` |
| **Formatos de Transporte** | Apenas JSON textual convencional | JSON e MessagePack binário ultraleve |
| **Roteamento** | `app.MapSignalR()` no OWIN Startup | `app.MapHub<THub>("/rota")` no pipeline Kestrel |
| **Streaming** | Não suportado de forma nativa | Suporte a `IAsyncEnumerable<T>` e Channels |
| **Reconexão** | Complexa e suscetível a loops | `withAutomaticReconnect()` nativo com backoff exponencial |

---

## 3. O Dilema do Hub Dinâmico vs Hub Fortemente Tipado

### No ASP.NET SignalR 2.x (Legado):
```csharp
// Herança não tipada: Clients.All é dynamic
public class NotificacoesLegadasHub : Microsoft.AspNet.SignalR.Hub
{
    public void EnviarAviso(string mensagem)
    {
        // Se houver erro de digitação ("ReceberAvisoo"), compila sem erro e quebra em silêncio no cliente
        Clients.All.ReceberAviso(mensagem, DateTime.Now.ToString("s"));
    }
}
```

### No .NET 10 com Hub&lt;TClient&gt; (Moderno):
```csharp
// Contrato fortemente tipado para os clientes conectados
public interface INotificacaoCliente
{
    Task ReceberNotificacao(NotificacaoEvento evento);
    Task AtualizarMetricas(MetricasTransmissao metricas);
}

// Hub fortemente tipado: segurança de tipo garantida pelo compilador C#
public sealed class NotificacoesHub : Hub<INotificacaoCliente>
{
    public async Task TransmitirAviso(string titulo, string mensagem)
    {
        var evento = new NotificacaoEvento(titulo, mensagem, DateTime.UtcNow);
        // O compilador verifica o método ReceberNotificacao e seus tipos
        await Clients.All.ReceberNotificacao(evento);
    }
}
```

---

## 4. Estrutura do Laboratório

```text
concepts/17-signalr/
├── README.md
├── net10/
│   ├── Properties/launchSettings.json   (Porta Kestrel 6700)
│   ├── Hubs/
│   │   ├── INotificacaoCliente.cs       (Interface de contrato para o cliente)
│   │   └── NotificacoesHub.cs           (Hub fortemente tipado com suporte a grupos)
│   ├── Models/                          (Modelos de eventos e métricas de conexão)
│   ├── Services/
│   │   ├── ITransmissorEventos.cs
│   │   └── TransmissorEventos.cs        (Serviço injetando IHubContext para broadcast)
│   ├── Controllers/
│   │   └── NotificacoesController.cs    (Endpoint REST que dispara eventos via IHubContext)
│   ├── wwwroot/
│   │   ├── js/signalr.min.js            (Cliente SignalR moderno sem jQuery)
│   │   ├── css/site.css
│   │   └── index.html                   (Painel de monitoramento de eventos em tempo real)
│   ├── Program.cs                       (AddSignalR, MapHub e injeção de dependências)
│   └── SignalRDemo.csproj
└── framework/
    ├── Properties/AssemblyInfo.cs
    ├── Hubs/
    │   └── NotificacoesLegadasHub.cs    (Hub clássico derivado de Microsoft.AspNet.SignalR.Hub)
    ├── Models/
    ├── Controllers/
    │   └── HomeController.cs
    ├── Startup.cs                       (Configuração OWIN com app.MapSignalR())
    ├── Views/Home/Index.cshtml          (Interface clássica demonstrando a dependência de jQuery)
    ├── Content/Site.css
    ├── Web.config                       (Porta IIS Express 6701)
    ├── packages.config                  (Microsoft.AspNet.SignalR 2.4.3 e Microsoft.Owin)
    └── SignalRDemo.csproj
```

---

## 5. Como Executar os Projetos

### Projeto Moderno (.NET 10)
1. Navegue até o diretório `concepts/17-signalr/net10/`.
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```text
   http://localhost:6700
   ```
4. Abra múltiplos navegadores ou abas simultâneas para observar a sincronização em tempo real de eventos, entrada e saída de grupos de canais e envio de mensagens a partir de requisições REST via `IHubContext`.

### Projeto Legado (.NET Framework 4.8.1)
1. Abra o arquivo de solução ou o projeto `SignalRDemo.csproj` localizado em `concepts/17-signalr/framework/` no Visual Studio 2022.
2. Inicie a execução sob o IIS Express (configurado para a porta `6701`).
3. Acesse a interface no navegador:
   ```text
   http://localhost:6701
   ```
