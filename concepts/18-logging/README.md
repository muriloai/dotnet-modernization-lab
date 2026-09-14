# Conceito 18: Logging e Observabilidade - Logging

Este laboratório apresenta uma análise prática e comparativa entre as estratégias de registro de logs no ecossistema clássico do **.NET Framework 4.8.1** e na arquitetura moderna do **.NET 10**.

---

## 1. Contexto e Motivação

Durante anos, o desenvolvimento em .NET Framework não dispunha de uma abstração de logging unificada fornecida pela própria plataforma. Aplicações corporativas dependiam de três caminhos principais:
1. Utilização das classes nativas primitivas do namespace `System.Diagnostics` (`Trace`, `Debug`, `TraceSource`), configuradas através de extensos blocos XML no `Web.config`.
2. Adoção direta de bibliotecas de terceiros como log4net, NLog ou Elmah, acoplando o código de negócio a APIs proprietárias e estáticas (por exemplo, `LogManager.GetLogger(...)`).
3. Formatação simples de mensagens por interpolação de strings (`$"{usuario} executou a ação {acao}"`) ou `string.Format`, o que gerava alta pressão no Garbage Collector por constantes alocações no heap e descartava a estrutura semântica dos dados.

No **.NET 10**, a infraestrutura de diagnóstico foi concebida a partir de contratos flexíveis e nativos:
- O pacote `Microsoft.Extensions.Logging` estabelece uma camada unificada de abstração baseada em `ILogger` e `ILogger<T>`, com suporte nativo ao contêiner de injeção de dependências.
- **Logging Estruturado (Semantic Logging):** Os templates de mensagens preservam os parâmetros como atributos nomeados (chave e valor), viabilizando consultas precisas e enriquecidas em agregadores modernos como OpenTelemetry, Elasticsearch, Seq e Application Insights.
- **Escopos de Execução (`BeginScope`):** Permitem associar dados contextuais (como identificadores de requisição HTTP, transação ou usuário) a todos os eventos disparados dentro de um bloco lógico.
- **Geradores de Código em Tempo de Compilação (`[LoggerMessage]`):** Eliminam alocações desnecessárias no heap (zero-allocation logging), evitando o boxing de tipos primitivos de valor e dispensando a análise de expressões de formato em tempo de execução.

---

## 2. Comparativo Técnico Direto

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Abstração Nativa** | Ausente; utilizava-se `System.Diagnostics.Trace` ou bibliotecas acopladas | `Microsoft.Extensions.Logging` (`ILogger`, `ILogger<T>`, `ILoggerFactory`) |
| **Injeção de Dependências** | Rara; loggers eram obtidos por campos estáticos (`LogManager.GetLogger`) | Nativa; loggers tipados são injetados diretamente no construtor |
| **Formato de Mensagens** | Texto plano via `string.Format` ou concatenação | Mensagens semânticas estruturadas com templates nomeados (`{PedidoId}`) |
| **Preservação de Propriedades** | Nenhuma; todos os valores são convertidos em string na hora | Total; parâmetros são exportados como pares chave-valor consultáveis |
| **Escopos Contextuais** | Inexistentes na plataforma nativa; exigia gerenciamento manual | Suporte nativo com `_logger.BeginScope` para enriquecer eventos |
| **Configuração de Filtros** | Blocos XML no `Web.config` via `<system.diagnostics>` | `appsettings.json` com níveis granulares por categoria e namespace |
| **Desempenho e Alocações** | Alocações frequentes por formatação de strings e boxing de tipos | Otimizado; suporte a `[LoggerMessage]` com alocação zero no heap |
| **Destinos (Sinks/Providers)** | `TraceListener` clássico associado ao processo do IIS | Múltiplos provedores intercambiáveis (Console, OpenTelemetry, Serilog) |

---

## 3. O Modelo Legado (.NET Framework 4.8.1)

No .NET Framework, o rastreamento nativo dependia de `System.Diagnostics.Trace`.

### Configuração em XML no `Web.config`
Toda a infraestrutura de escuta precisava ser descrita de maneira verbosa em XML:

```xml
<system.diagnostics>
  <trace autoflush="true">
    <listeners>
      <add name="ArquivoTrace"
           type="System.Diagnostics.TextWriterTraceListener"
           initializeData="App_Data/trace.log" />
      <add name="ListenerMemoria"
           type="LoggingDemo.Services.MemoriaTraceListener, LoggingDemo" />
    </listeners>
  </trace>
</system.diagnostics>
```

### O Desafio da Concatenação e Falta de Estrutura
O código de negócio invocava métodos estáticos e gerava strings brutas:

```csharp
// Exemplo clássico em controlador ou serviço do .NET Framework:
Trace.TraceInformation("Processando pedido {0} para o cliente {1} no valor de {2:C}", pedidoId, clienteId, total);

try
{
    // Lógica de processamento
}
catch (Exception ex)
{
    Trace.TraceError("Falha crítica ao processar pedido " + pedidoId + ": " + ex.Message);
}
```

Limitações deste formato:
1. **Perda da semântica:** Em um sistema de análise de logs, buscar "todos os pedidos do cliente 42 com valor superior a 500" exige expressões regulares custosas sobre o texto plano gerado.
2. **Pressão no coletor de lixo:** Cada concatenação ou chamada a `string.Format` aloca novos objetos na memória gerenciada, reduzindo a vazão geral de requisições simultâneas.
3. **Dificuldade de correlação:** Se dez requisições simultâneas geram entradas de log, não há um identificador nativo que vincule os passos de uma mesma operação sem implementação manual de cabeçalhos e variáveis locais de thread.

---

## 4. O Modelo Moderno (.NET 10)

No .NET 10, o logging é parte integrante da arquitetura do host e da injeção de dependências.

### Logging Estruturado e Injeção de Dependências
O serviço recebe `ILogger<ProcessadorPedidosService>` no construtor e utiliza marcadores nomeados:

```csharp
public class ProcessadorPedidosService
{
    private readonly ILogger<ProcessadorPedidosService> _logger;

    public ProcessadorPedidosService(ILogger<ProcessadorPedidosService> logger)
    {
        _logger = logger;
    }

    public void Processar(PedidoOperacao pedido, string correlationId)
    {
        // Criação de escopo contextual
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ClienteId"] = pedido.ClienteId
        }))
        {
            // O template armazena as propriedades PedidoId e ValorTotal de forma independente
            _logger.LogInformation(
                "Iniciando faturamento do pedido {PedidoId} no total de {ValorTotal}",
                pedido.PedidoId,
                pedido.ValorTotal);
        }
    }
}
```

### Logging de Alta Performance com `[LoggerMessage]`
Para cenários de alto throughput onde até mesmo o custo de analisar templates de string é indesejado, o .NET 10 disponibiliza o gerador de código via atributo `[LoggerMessage]`:

```csharp
public static partial class LoggerOtimizado
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Pedido {pedidoId} confirmado com sucesso para o canal {canal}")]
    public static partial void LogPedidoConfirmado(
        ILogger logger,
        int pedidoId,
        string canal);
}
```

O compilador C# gera automaticamente um delegate altamente especializado que grava os argumentos sem qualquer alocação no heap e sem realizar boxing em tipos primitivos como inteiros e booleanos.

---

## 5. Estrutura deste Laboratório

```text
concepts/18-logging/
├── README.md
├── framework/
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Models/
│   │   ├── LogLegadoItem.cs
│   │   └── PedidoLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   ├── MemoriaTraceListener.cs
│   │   └── ProcessadorPedidosLegado.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── LoggingDemo.csproj
│   ├── packages.config
│   └── Web.config
└── net10/
    ├── Controllers/
    │   ├── LogsInspecaoController.cs
    │   └── PedidosController.cs
    ├── Models/
    │   ├── LogItemVisualizacao.cs
    │   ├── PedidoOperacao.cs
    │   └── RequisicaoProcessamento.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   ├── IProvedorLogsMemoria.cs
    │   ├── LoggerOtimizado.cs
    │   ├── ProcessadorPedidosService.cs
    │   └── ProvedorLogsMemoria.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── site.css
    │   └── index.html
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── LoggingDemo.csproj
    └── Program.cs
```

---

## 6. Como Executar os Projetos

### Executando o Projeto Moderno (.NET 10)
1. Abra o terminal na pasta `concepts/18-logging/net10`.
2. Execute o comando:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador em `http://localhost:6800`.
4. No painel, dispare pedidos normais, alertas de estoque e falhas simuladas. Inspecione os eventos estruturados, o enriquecimento de propriedades pelos escopos e a execução dos logs gerados em tempo de compilação.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra a pasta `concepts/18-logging/framework` no Visual Studio 2022 em um ambiente Windows com suporte a ASP.NET.
2. Inicie o projeto através do IIS Express na porta `6801`.
3. Acesse `http://localhost:6801` para observar a emissão de mensagens via `System.Diagnostics.Trace` e a perda de granularidade dos dados.
