# Conceito 20: Logging e Observabilidade - Health Checks e Diagnósticos

Este laboratório analisa as estratégias de verificação de integridade e diagnóstico de integridade da aplicação, contrastando as soluções manuais do **.NET Framework 4.8.1** com o ecossistema nativo de Health Checks do **.NET 10**.

---

## 1. Contexto e Motivação

Durante o ciclo de vida do .NET Framework em servidores Windows tradicionais (IIS), não existia um padrão de plataforma para verificação de saúde da aplicação:
1. **Soluções ad-hoc e proprietárias:** Para atender balanceadores de carga (como F5, HAProxy ou AWS ELB), desenvolvedores criavam arquivos de extensão `.ashx` (`IHttpHandler`) como `ping.ashx` ou controladores como `StatusController`.
2. **Respostas rudimentares:** Tais endpoints costumavam retornar uma string estática `"OK"` ou códigos HTTP 200/500 após blocos manuais de `try-catch` executando `SELECT 1` no banco de dados.
3. **Ausência de distinção entre Liveness e Readiness:** Se o banco de dados ficasse temporariamente indisponível durante uma janela de backup, o endpoint de status falhava. Em balanceadores simples ou orquestradores, isso podia provocar a reinicialização em loop do processo (reciclagem do AppPool), agravando o problema em vez de apenas desviar o tráfego de usuários.
4. **Dependência do sistema operacional:** Métricas e diagnósticos dependiam de contadores de desempenho do Windows (`PerformanceCounter`) e do Visualizador de Eventos (`EventLog`), mecanismos fortemente acoplados ao Windows Server e inviáveis em contêineres Linux leves.

No **.NET 10**, o diagnóstico e a checagem de saúde são tratados como componentes centrais da arquitetura de nuvem:
- **Infraestrutura Nativa de Health Checks:** O middleware `Microsoft.AspNetCore.Diagnostics.HealthChecks` fornece contratos extensíveis baseados na interface `IHealthCheck`, com suporte a timeouts granulares e cancelamento assíncrono via `CancellationToken`.
- **Estados Semânticos de Saúde:** O modelo define três estados formais: `Healthy` (saudável), `Degraded` (degradado, operando com latência ou redundância reduzida) e `Unhealthy` (inoperante).
- **Segregação Rigorosa de Probes:**
  - **Liveness Probe (`/health/live`):** Avalia se o processo da aplicação está vivo, sem bloqueios de thread ou consumo crítico de recursos essenciais. Se falhar, o orquestrador (como Kubernetes) realiza o restart do contêiner.
  - **Readiness Probe (`/health/ready`):** Avalia se dependências externas vitais (bancos de dados, filas, caches) estão acessíveis. Se falhar, o orquestrador apenas remove o pod do balanceamento de tráfego, aguardando a recuperação sem reiniciar o processo.
- **Formatação de Resposta Estruturada:** Capacidade de gerar relatórios detalhados em JSON com tempos individuais de execução de cada dependência e metadados de diagnóstico.

---

## 2. Comparativo Técnico Direto

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Suporte Nativo da Plataforma** | Inexistente; exigia construção manual de handlers `.ashx` ou controllers | Nativo via `services.AddHealthChecks()` e `app.MapHealthChecks()` |
| **Contrato de Verificação** | Métodos avulsos sem interface padronizada | Interface unificada `IHealthCheck` com `CheckHealthAsync` |
| **Estados de Integridade** | Binário informal (200 OK ou 500 Erro) | Tipado: `Healthy`, `Degraded` e `Unhealthy` |
| **Segregação de Probes** | Nenhuma; o mesmo endpoint era usado para liveness e dependências | Nativa; filtragem por tags (`live`, `ready`) em rotas segregadas |
| **Controle de Timeout** | Manual em cada comando de conexão | Configurável por verificação diretamente no registro |
| **Diagnósticos de Sistema** | `PerformanceCounter` e Event Viewer do Windows Server | `System.Diagnostics.Metrics`, OpenTelemetry e contadores multiplataforma |
| **Compatibilidade com Contêineres** | Difícil; dependente de IIS e arquitetura Windows | Nativa; aderente aos padrões de sondas do Kubernetes |

---

## 3. O Modelo Legado (.NET Framework 4.8.1)

No modelo tradicional, o desenvolvedor implementava um handler `IHttpHandler` específico:

```csharp
public class PingHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        
        try
        {
            // Verificacao manual e sincronica sem suporte a cancelamento
            using (var conexao = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDb"].ConnectionString))
            {
                conexao.Open();
                using (var cmd = new SqlCommand("SELECT 1", conexao))
                {
                    cmd.ExecuteScalar();
                }
            }
            
            context.Response.Write("OK");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("ERRO: " + ex.Message);
        }
    }
    
    public bool IsReusable => true;
}
```

Limitações deste formato:
1. Uma lentidão temporária no banco de dados bloqueava threads do IIS indefinidamente, pois não havia timeout de verificação desacoplado do timeout da conexão ADO.NET.
2. Não havia informações sobre dependências secundárias (por exemplo, um serviço de mensageria fora do ar que degrada a aplicação, mas não inviabiliza consultas).
3. O orquestrador de infraestrutura não conseguia discernir se o processo travou ou se apenas um serviço externo está indisponível.

---

## 4. O Modelo Moderno (.NET 10)

No .NET 10, cada componente de infraestrutura possui seu próprio `IHealthCheck`.

### Implementação de um Health Check de Banco de Dados com Estado Degradado
```csharp
public class BancoDadosHealthCheck : IHealthCheck
{
    private readonly ISimuladorEstadoRecursos _simulador;

    public BancoDadosHealthCheck(ISimuladorEstadoRecursos simulador)
    {
        _simulador = simulador;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var latencia = _simulador.ObterLatenciaBanco();

        if (!_simulador.BancoDisponivel)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("O banco de dados relacional está inacessível."));
        }

        if (latencia > TimeSpan.FromMilliseconds(500))
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Banco operacional, porém com alta latência detectada: {latencia.TotalMilliseconds}ms."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy("Banco de dados operando normalmente com latência aceitável."));
    }
}
```

### Configuração e Mapeamento de Probes (`Program.cs`)
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<MemoriaHealthCheck>("memoria", tags: ["live", "ready"])
    .AddCheck<BancoDadosHealthCheck>("banco_dados", tags: ["ready"])
    .AddCheck<ServicoMensageriaHealthCheck>("mensageria", tags: ["ready"]);

var app = builder.Build();

// Endpoint de liveness: avalia apenas o processo e memoria
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriters.EscreverRespostaJson
});

// Endpoint de readiness: avalia dependencias para recebimento de trafego
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriters.EscreverRespostaJson
});
```

Diferencial decisivo:
- Se o banco de dados falhar, `/health/ready` responde `503 Service Unavailable` e o Kubernetes retira o contêiner do pool de tráfego.
- Ao mesmo tempo, `/health/live` continua respondendo `200 OK`, impedindo que o contêiner seja destruído e recriado em loop desnecessariamente.

---

## 5. Estrutura deste Laboratório

```text
concepts/20-health-checks/
├── README.md
├── framework/
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   └── StatusController.cs
│   ├── Handlers/
│   │   ├── PingHandler.ashx
│   │   └── PingHandler.ashx.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── HealthChecksDemo.csproj
│   ├── packages.config
│   └── Web.config
└── net10/
    ├── Checks/
    │   ├── BancoDadosHealthCheck.cs
    │   ├── MemoriaHealthCheck.cs
    │   └── ServicoMensageriaHealthCheck.cs
    ├── Controllers/
    │   └── SimuladorController.cs
    ├── Formatters/
    │   └── HealthCheckResponseWriters.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   ├── ISimuladorEstadoRecursos.cs
    │   └── SimuladorEstadoRecursos.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── site.css
    │   └── index.html
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── HealthChecksDemo.csproj
    └── Program.cs
```

---

## 6. Como Executar os Projetos

### Executando o Projeto Moderno (.NET 10)
1. Abra o terminal na pasta `concepts/20-health-checks/net10`.
2. Execute o comando:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador em `http://localhost:7000`.
4. No painel, alterne o estado do banco de dados (Saudável, Degradado, Inoperante) e da mensageria. Observe como `/health/live` permanece verde (evitando reinicialização do processo) enquanto `/health/ready` e `/health` reagem com status correspondentes e respostas JSON ricas.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra a pasta `concepts/20-health-checks/framework` no Visual Studio 2022 em um ambiente Windows com carga ASP.NET instalada.
2. Inicie o projeto através do IIS Express na porta `7001`.
3. Acesse `http://localhost:7001` para testar as rotas legadas `/ping.ashx` e `/Status` e constatar as limitações do formato binário sem segregação de liveness/readiness.
