# .NET Modernization Lab

Laboratório prático de modernização de código comparando, lado a lado, o ecossistema legado **.NET Framework 4.8.1** e ecossistema moderno do **.NET 10**.

O objetivo deste repositório é demonstrar como funcionalidades equivalentes eram construídas no modelo clássico do ASP.NET e como são implementadas com os recursos atuais da plataforma .NET e do C# 14.

---

## Arquitetura do Repositório: Conceitos Isolados

Cada conceito técnico possui sua própria pasta independente dentro do diretório `concepts/`, contendo dois mini-projetos funcionais:

```
concepts/
├── 01-project-structure/
│   ├── README.md               # Explicação técnica e comparativo do conceito
│   ├── framework/              # Projeto ASP.NET (.NET Framework 4.8.1)
│   └── net10/                  # Projeto ASP.NET Core (.NET 10)
├── 02-configuration/
│   ├── README.md
│   ├── framework/
│   └── net10/
└── ...
```

Desta forma, cada laboratório pode ser compilado, executado e inspecionado individualmente, sem interferência de outros módulos.

---

## Módulos e Conceitos do Laboratório

### Fundamentos da Plataforma

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [01](concepts/01-project-structure/README.md) | Estrutura de Projeto e Inicialização | MSBuild clássico XML, `Global.asax.cs`, IIS                   | SDK-style csproj, `Program.cs` com Top-Level, Kestrel                   |
| [02](concepts/02-configuration/README.md)     | Configuração e Options Pattern       | `ConfigurationManager`, XML `Web.config`, seções customizadas | `IConfiguration`, JSON hierárquico, Options Pattern e `IOptionsMonitor` |
| [03](concepts/03-dependency-injection/README.md) | Injeção de Dependências             | Sem DI nativo, containers externos e Service Locator          | Container de injeção nativo, ciclos de vida e Keyed Services            |
| [04](concepts/04-middleware-vs-modules/README.md) | Middlewares vs Módulos HTTP         | IHttpModule, IHttpHandler e eventos do IIS                    | Pipeline linear de middlewares com RequestDelegate                      |
| [05](concepts/05-routing/README.md)           | Roteamento                           | Tabelas separadas (MVC vs Web API) e conflitos de convenção   | Endpoint Routing unificado, MapGroup e restrições tipadas                |
| [06](concepts/06-controllers-actions/README.md) | Controllers e Actions               | Divisão entre Controller (MVC) e ApiController (Web API)      | ControllerBase unificado, [ApiController] e TypedResults                 |
| [07](concepts/07-views-razor-templating/README.md) | Views, Razor e Templating           | Razor 3, HTML Helpers (@Html.*), Child Actions e MVC puro    | Tag Helpers (<input asp-*>), View Components, Razor Pages                |

### Dados e Persistência

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [08](concepts/08-entity-framework/README.md)  | Entity Framework: EF 6 vs EF Core 10 | DbContext clássico, Web.config, inicializadores e roundtrips individuais | EF Core 10, AddDbContext, AsSplitQuery, ExecuteUpdate/Delete e batching nativo |
| [09](concepts/09-repository-pattern/README.md) | Padrão Repositório e Unit of Work    | Repositórios genéricos (IRepository<T>), vazamento de IQueryable e UoW manual | Uso direto do DbContext com DI, repositórios de domínio (DDD) e transações assíncronas |
| [10](concepts/10-data-validation/README.md)    | Validação de Dados e ProblemDetails  | DataAnnotations, checagem manual de ModelState.IsValid e erro proprietário | [ApiController] com validação automática, RFC 7807 ProblemDetails e IValidatableObject |

### Segurança

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [11](concepts/11-authentication/README.md)    | Autenticação                         | FormsAuthentication, MachineKey, cookies proprietários e IPrincipal | AddAuthentication, Cookie e JWT Bearer, ClaimsPrincipal e SignInAsync     |
| [12](concepts/12-authorization-policies/README.md) | Autorização e Políticas              | [Authorize(Roles = "Admin")], papéis estáticos e checagens manuais no código | AddAuthorization, Policy-based Authorization, IAuthorizationRequirement e handlers |
| [13](concepts/13-security-best-practices/README.md) | Práticas de Segurança                | AntiForgery manual, cabeçalhos em Web.config e risco do BinaryFormatter | UseAntiforgery, AddCors tipado, Security Headers em middleware e System.Text.Json |

### HTTP, APIs e Comunicação

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [14](concepts/14-rest-apis/README.md)         | Construção de APIs REST              | ApiController (Web API 2), IHttpActionResult e rotas manuais  | [ApiController], ControllerBase, IResult/TypedResults, OpenAPI nativo e Minimal APIs |
| [15](concepts/15-httpclient/README.md)        | HttpClient e Requisições Externas    | WebClient, HttpWebRequest, new HttpClient() e socket exhaustion | IHttpClientFactory, Typed Clients, SocketsHttpHandler e resiliência     |
| [16](concepts/16-action-filters/README.md)    | Action Filters                       | ActionFilterAttribute em MVC e Web API separados, sem DI e Service Locator | IAsyncActionFilter, ServiceFilter, TypeFilter, DI nativa e Endpoint Filters |
| [17](concepts/17-signalr/README.md)           | Comunicação em Tempo Real com SignalR | ASP.NET SignalR 2.x, OWIN, dependência de jQuery e proxies mágicos | ASP.NET Core SignalR, Hub fortemente tipado, MessagePack e sem jQuery   |

### Logging e Observabilidade

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [18](concepts/18-logging/README.md)           | Logging                              | System.Diagnostics.Trace, listeners em Web.config e strings  | Microsoft.Extensions.Logging, ILogger<T>, structured logging e scopes    |
| [19](concepts/19-error-handling/README.md)    | Tratamento de Erros                  | Application_Error, HandleErrorAttribute e customErrors no Web.config | IExceptionHandler, UseExceptionHandler e RFC 7807/9457 ProblemDetails   |
| [20](concepts/20-health-checks/README.md)     | Health Checks e Diagnósticos         | Handlers manuais .ashx (ping/status), SELECT 1 e PerformanceCounters | AddHealthChecks, IHealthCheck, liveness/readiness probes e métricas nativas |

### Serialização, Cache e Desempenho

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [21](concepts/21-json-serialization/README.md) | Serialização JSON                    | JavaScriptSerializer, Newtonsoft.Json (Json.NET) e reflexão pesada | System.Text.Json, Utf8JsonReader/Writer, Source Generators e streaming IAsyncEnumerable |
| [22](concepts/22-caching/README.md)           | Estratégias de Cache                 | System.Web.Caching.Cache, OutputCache em MVC, MemoryCache estático e sem tags | IMemoryCache, IDistributedCache, OutputCache middleware com Tags e eviction seletiva |
| [23](concepts/23-performance/README.md)       | Otimizações de Desempenho            | Alocações com Substring, boxing/unboxing, novos byte[] e GC pauses no Windows | Span<T>, ReadOnlySpan<T>, ArrayPool<T>, Zero-Allocation parsing e GC moderno |

### Evolução da Linguagem C#

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| [24](concepts/24-csharp-evolution/README.md)  | Evolução da Linguagem C# (7.3 vs 14) | C# 7.3, classes prolixas, switch clássico, sem records e indentação aninhada | C# 14, Records, Pattern Matching avançado, Raw Strings, Collection Expressions e Primary Constructors |
| [25](concepts/25-async-await/README.md)       | Evolução de Async e Await            | AspNetSynchronizationContext, ConfigureAwait(false), Task.Result deadlocks e Task<List<T>> | Sem SynchronizationContext, ValueTask<T>, IAsyncEnumerable<T>, await foreach e Task.WhenEach |

---

## Pré-requisitos para Execução

### Para os projetos em .NET 10:

- [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado.
- Terminal compatível com a CLI `dotnet` (PowerShell, Bash ou Prompt de Comando).
- Editor ou IDE de sua preferência (Visual Studio 2022, Visual Studio Code ou JetBrains Rider).

### Para os projetos em .NET Framework 4.8.1:

- Sistema Operacional Windows.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) com a carga de trabalho **ASP.NET e desenvolvimento web** instalada (necessário para o runtime IIS Express e referências do MSBuild clássico).
- Developer Pack do .NET Framework 4.8.1.

---

## Como Executar os Laboratórios

### Executando o lado moderno (.NET 10)

Abra o terminal na pasta do conceito desejado e utilize os comandos da CLI:

```bash
cd concepts/01-project-structure/net10
dotnet run
```

### Executando o lado legado (.NET Framework 4.8.1)

1. Abra o arquivo de projeto (`.csproj`) ou a solução correspondente no Visual Studio 2022.
2. Defina o projeto legado como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e inicializar no IIS Express.

Para detalhes específicos de cada laboratório, consulte o arquivo `README.md` localizado na pasta de cada conceito.
