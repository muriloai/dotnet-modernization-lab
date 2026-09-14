# Conceito 27: Background Services e Hosted Services

Este laboratório compara a execução de tarefas em segundo plano entre o ASP.NET MVC no .NET Framework 4.8.1 e os Hosted Services no .NET 10.

---

## 1. Cenário e Justificativa

Em aplicações web, é comum a necessidade de executar tarefas assíncronas desacopladas do ciclo de vida da requisição HTTP: processamento de lotes, envio de notificações em fila, limpeza periódica de recursos e consumo de eventos.

### No .NET Framework 4.8.1 (Legado)
* **Reciclagem do IIS:** O processo do IIS (w3wp.exe) e os Application Pools podem ser reciclados por inatividade, limites de memória ou horários programados. Qualquer thread em execução em segundo plano sem controle explícito é sumariamente abortada.
* **Timers e Threads soltas:** A criação de instâncias de `System.Threading.Timer` ou novas `Thread` em `Application_Start` no `Global.asax` constitui um anti-padrão grave, suscetível a vazamentos de memória e encerramento desordenado.
* **HostingEnvironment.QueueBackgroundWorkItem (QBWI):** Introduzido no .NET 4.5.2, permitia agendar uma tarefa e notificar o IIS para estender o tempo de shutdown (geralmente 30 segundos), mas carecia de injeção de dependências, tratamento robusto de ciclo de vida e propagação adequada de cancelamento.
* **Dependência de Serviços Externos:** Para tarefas confiáveis, equipes eram forçadas a criar Windows Services separados via `TopShelf` ou executáveis de console agendados no Windows Task Scheduler.

### No .NET 10 (Moderno)
* **IHostedService e BackgroundService:** O Generic Host gerencia nativamente componentes que executam em segundo plano integrados ao ciclo de vida da aplicação.
* **PeriodicTimer:** Introduzido para substituir timers legados, operando de forma assíncrona (`await timer.WaitForNextTickAsync(stoppingToken)`), sem retenção de threads e sem acúmulo de ticks.
* **System.Threading.Channels:** Filas produtor-consumidor de altíssimo desempenho e seguras para concorrência (`Channel<T>`), permitindo que controllers enfileirem mensagens sem bloqueios e que workers as processem de forma controlada.
* **Graceful Shutdown:** `IHostApplicationLifetime` coordena a parada graciosa, permitindo drenar filas e finalizar conexões de banco de dados antes do processo encerrar.
* **Unificação:** A mesma arquitetura pode ser executada como serviço web (Kestrel), Worker Service (daemon de console) ou serviço do sistema operacional (systemd / Windows Service) apenas alterando o pacote de hospedagem.

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Abstração Base** | Nenhuma nativa ou `QueueBackgroundWorkItem` | `IHostedService` e `BackgroundService` |
| **Hospedagem** | IIS (w3wp.exe) ou Windows Service separado | Generic Host (Kestrel, Worker, systemd, Windows Service) |
| **Ciclo de Vida e Parada** | Abort abrupto em reciclagens do AppPool | Encerramento gracioso cooperativo via `CancellationToken` |
| **Temporizador** | `System.Threading.Timer` (callbacks em ThreadPool) | `PeriodicTimer` (assíncrono, sem drift, zero alocações) |
| **Comunicação Concorrente** | `BlockingCollection<T>` ou tabelas de banco | `System.Threading.Channels.Channel<T>` |
| **Injeção de Dependências** | Manual ou Service Locator estático | Totalmente integrada com controle de escopo (`IServiceScopeFactory`) |
| **Resiliência** | Difícil diagnóstico de exceções não capturadas | `BackgroundServiceExceptionBehavior` configurável |

---

## 3. Estrutura dos Projetos

```
concepts/27-background-services/
├── README.md
├── framework/
│   ├── BackgroundServicesDemo.csproj
│   ├── Global.asax / Global.asax.cs
│   ├── Web.config
│   ├── Models/
│   │   └── TarefaSegundoPlanoLegada.cs
│   ├── Services/
│   │   └── FilaProcessamentoLegada.cs
│   ├── Controllers/
│   │   └── BackgroundTasksLegadoController.cs
│   └── Views/
│       └── BackgroundTasksLegado/Index.cshtml
└── net10/
    ├── BackgroundServicesDemo.csproj
    ├── Program.cs
    ├── Models/
    │   └── TarefaSegundoPlano.cs
    ├── Services/
    │   ├── IFilaProcessamento.cs
    │   └── FilaProcessamentoChannel.cs
    ├── Workers/
    │   ├── FilaProcessamentoWorker.cs
    │   └── MonitoramentoPeriodicoWorker.cs
    ├── Controllers/
    │   └── BackgroundTasksController.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 7700)
```powershell
cd concepts/27-background-services/net10
dotnet run
```
Acesse no navegador: `http://localhost:7700`

### .NET Framework 4.8.1 (Porta 7701)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\27-background-services\framework /port:7701
```
Acesse no navegador: `http://localhost:7701`
