# Conceito 35: OpenTelemetry e Aspire no .NET 10 vs Diagnósticos Legados no .NET Framework 4.8.1

Este laboratório demonstra a revolução da observabilidade em ambientes de microsserviços e aplicações distribuídas, comparando as práticas antigas do .NET Framework 4.8.1 (baseadas em `System.Diagnostics.TraceSource`, contadores do Windows PerfMon e logs em arquivos de texto desestruturados) com o padrão unificado **OpenTelemetry (OTLP)** e o ecossistema **.NET Aspire** no .NET 10.

---

## 1. Visão Geral e Contexto Prático

Com a proliferação de arquiteturas em nuvem e microsserviços, monitorar sistemas exige três pilares integrados de observabilidade:

1. **Distributed Tracing (Rastreamento Distribuído):** Capacidade de acompanhar uma requisição fim a fim ao atravessar múltiplos serviços, bancos de dados e filas de mensageria, correlacionando latências e dependências sob o mesmo TraceId.
2. **Metrics (Métricas Dimensionadas):** Contadores, medidores e histogramas que medem a saúde do sistema e volume de transações sem impactar a latência da thread de processamento.
3. **Structured Logging (Logs Estruturados):** Registros enriquecidos com atributos tipados que herdam o identificador de contexto do trace ativo.

---

## 2. O Cenário Legado no .NET Framework 4.8.1

No .NET Framework 4.8.1, a instrumentação enfrentava obstáculos severos:

* **Ausência de Padrão Aberto de Rastreamento:** Não havia suporte nativo ao padrão W3C Trace Context (`traceparent`). Quando um cliente disparava uma requisição que passava por mais de um serviço, os desenvolvedores criavam headers HTTP proprietários (como `X-Correlation-ID`) e repassavam manualmente.
* **Diagnósticos Frágeis do Windows (PerfMon):** As métricas dependiam da infraestrutura de `PerformanceCounter` do Windows. Criar contadores personalizados exigia privilégios de Administrador da máquina, registro no Registro do Windows e instalação de pacotes no sistema operacional, inviabilizando contêineres e nuvem.
* **Logs Desestruturados e Isolados:** As bibliotecas clássicas (log4net, NLog antigo) gravavam arquivos de texto locais em `C:\Logs\app.log`. A análise de falhas exigia coletar arquivos texto manualmente de cada servidor IIS ou usar agentes de SO com alto consumo de disco e CPU.
* **Falta de Árvore de Spans (Waterfall):** Identificar se um gargalo de 500ms ocorreu no Gateway de Pagamento, na query do banco ou na mensageria exigia comparar timestamps imprecisos de linhas de texto soltas.

---

## 3. A Solução Moderna no .NET 10: OpenTelemetry & Aspire

No .NET 10, a observabilidade foi totalmente incorporada nas APIs fundamentais do runtime (BCL):

* **System.Diagnostics.ActivitySource Nativo:** A própria biblioteca base do .NET fornece as APIs de tracing. Criar um span (`Activity`) não requer bibliotecas pesadas de terceiros nem gera alocações excessivas de memória no heap.
* **W3C Trace Context Automático:** Toda requisição processada pelo ASP.NET Core recebe ou propaga automaticamente o cabeçalho W3C `traceparent` no formato `00-{traceId}-{spanId}-{flags}`, mantendo o contexto íntegro entre instâncias, inclusive em chamadas via `HttpClient`.
* **System.Diagnostics.Metrics:** Substitui o PerfMon por instrumentos modernos padronizados (`Counter<T>`, `Histogram<T>`, `ObservableGauge<T>`) compatíveis com Prometheus, Datadog e Grafana.
* **Padronização com .NET Aspire Service Defaults:** O ecossistema .NET Aspire padroniza a configuração de OpenTelemetry através de um conjunto de extensões reutilizáveis (`AddServiceDefaults`), permitindo que traces, métricas e logs sejam exportados via protocolo OTLP (gRPC/Protobuf) para coletores centrais com poucas linhas de código.

---

## 4. Comparativo de Código

### Rastreamento Manual Legado (.NET Framework 4.8.1)

```csharp
// Abordagem ad-hoc no .NET Framework 4.8.1
var correlationId = Guid.NewGuid().ToString("D");
Trace.WriteLine(string.Format("[{0}] Iniciando operacao de checkout", correlationId));

// Chamada manual de gateway sem propagacao W3C
using (var client = new WebClient())
{
    client.Headers.Add("X-Correlation-ID", correlationId);
    client.DownloadString("http://api-pagamento/pay");
}
```

### Rastreamento com OpenTelemetry (.NET 10)

```csharp
// OpenTelemetry nativo com ActivitySource no .NET 10
private static readonly ActivitySource ActivitySource = new("ModernizationLab.OrderService");

using (var activity = ActivitySource.StartActivity("CheckoutOrder", ActivityKind.Server))
{
    activity?.SetTag("order.id", orderId);
    activity?.SetTag("customer.name", cliente);

    // Spans filhos e chamadas HttpClient herdam o traceparent automaticamente!
    await ProcessarPagamentoAsync();
}
```

---

## 5. Tabela Comparativa Estrutural

| Característica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Padrão de Rastreamento** | Nenhum (CorrelationId manual proprietário) | Padrão internacional W3C Trace Context (`traceparent`) |
| **API de Spans** | `System.Diagnostics.TraceSource` descontinuado | `System.Diagnostics.ActivitySource` e `Activity` nativo |
| **Métricas** | Windows Performance Counters (`PerformanceCounter`) | `System.Diagnostics.Metrics` (`Meter`, `Counter`, `Histogram`) |
| **Protocolo de Exportação** | Arquivos locais em disco ou visualizadores de eventos | OpenTelemetry Protocol (OTLP) via gRPC ou HTTP/Protobuf |
| **Suporte a Nuvem / Contêineres** | Difícil (dependente de privilégios de admin no Windows) | 100% nativo em Linux, contêineres e Kubernetes |
| **Integração com Dashboard** | Nenhuma interface integrada out-of-the-box | Dashboard visual em tempo real via .NET Aspire / Jaeger |

---

## 6. Estrutura dos Projetos

```text
concepts/35-opentelemetry/
|-- README.md
|-- framework/
|   |-- App_Start/
|   |   `-- RouteConfig.cs
|   |-- Content/
|   |   `-- Site.css
|   |-- Controllers/
|   |   `-- TraceLegadoController.cs
|   |-- Models/
|   |   `-- PedidoLegadoDto.cs
|   |-- Views/
|   |   |-- Shared/
|   |   |   `-- _Layout.cshtml
|   |   `-- TraceLegado/
|   |       `-- Index.cshtml
|   |-- Global.asax
|   |-- TraceLegadoDemo.csproj
|   `-- Web.config
`-- net10/
    |-- Models/
    |   `-- TelemetryModels.cs
    |-- Properties/
    |   `-- launchSettings.json
    |-- Services/
    |   `-- OrderProcessingService.cs
    |-- wwwroot/
    |   |-- css/
    |   |   `-- site.css
    |   `-- index.html
    |-- OpenTelemetryDemo.csproj
    |-- Program.cs
    `-- appsettings.json
```

---

## 7. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/35-opentelemetry/net10
   ```
2. Execute a aplicação:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   `http://localhost:8500`
4. Experimente os seguintes cenários no painel:
   * Clique em **Disparar Checkout com Distributed Tracing**.
   * Observe a geração do cabeçalho **W3C traceparent** e a renderização imediata da **árvore de spans em cascata (waterfall)**: `CheckoutOrder` -> `ValidatePayment` -> `UpdateInventory` -> `DispatchNotification`.
   * Inspecione as tags em cada span e veja as métricas dimensionadas atualizadas em tempo real.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra o arquivo `TraceLegadoDemo.csproj` no Visual Studio 2022.
2. Inicie a aplicação com IIS Express na porta configurada:
   `http://localhost:8501`
3. Dispare um pedido no legado e compare as linhas de log de texto plano sem árvore de spans com o visualizador estruturado do .NET 10.
