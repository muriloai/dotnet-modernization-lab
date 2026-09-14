# Resumo Consolidado das Mudancas de Modernizacao (.NET 4.8.1 para .NET 10)

Este documento sintetiza os saltos tecnologicos e arquiteturais que ocorreram entre o **.NET Framework 4.8.1** (a versao final do framework Windows historico) e o **.NET 10** (a versao moderna de altissima performance, cross-platform e cloud-native).

---

## 1. Sistema de Projetos e Ferramentas de Build

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Formato de Projeto** | MSBuild classico prolixo (centenas de linhas com GUIDs, caminhos absolutos e listagem manual de cada arquivo `.cs`). | SDK-Style limpo e declarativo (`<Project Sdk="Microsoft.NET.Sdk.Web">`), com inclusao automatica de arquivos. |
| **Gerenciamento de Pacotes** | `packages.config` com pasta `packages/` local pesada ou `PackageReference` retroajustado sem suporte total. | `PackageReference` nativo e centralizado, com transitividade de dependencias e cache global no usuario. |
| **CLI e Linha de Comando** | Fragmentada (`msbuild.exe`, `nuget.exe`, `vstest.console.exe`). | CLI unificada e multiplataforma atraves do utilitario `dotnet` (`build`, `run`, `test`, `publish`, `tool`). |
| **Multi-Targeting** | Inviavel nativamente sem hacks no arquivo `.csproj`. | Suporte nativo e simplificado via tag `<TargetFrameworks>`. |

---

## 2. Modelo de Hospedagem e Runtime

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Servidor Web** | Acoplamento quase total com o Internet Information Services (IIS) e processo `w3wp.exe`. | Servidor Kestrel embutido, assincrono, com suporte nativo a HTTP/1.1, HTTP/2, HTTP/3 e gRPC. |
| **Sistemas Operacionais** | Exclusivo do Microsoft Windows. | 100% compativel com Linux, macOS e Windows. |
| **Tamanho de Imagens de Container** | Imagens Windows Server de 6 GB a 12 GB, com boot de minutos. | Imagens Linux Chiseled e Alpine de 30 MB a 120 MB, com boot em milissegundos. |
| **Compilacao Nativa** | JIT em tempo de execucao com alto cold start; NGen fragil preso a maquina local. | Native AOT (`<PublishAot>`), gerando binarios nativos sem necessidade de runtime CLR instalado. |

---

## 3. Arquitetura da Web e APIs

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Pipeline HTTP** | `IHttpModule` e `IHttpHandler` acoplados ao ciclo de eventos sincrono de 23 estagios do IIS. | Pipeline encadeado de Middlewares bidirecionais assincronos baseados em `RequestDelegate`. |
| **Construcao de APIs** | ASP.NET Web API 2 com heranca obrigatoria de `ApiController` e sobrecarga de alocacao de controllers. | Minimal APIs de alto rendimento (`app.MapGet`), Route Groups, Endpoint Filters e TypedResults. |
| **Interfaces de Usuario** | ASP.NET Web Forms com `__VIEWSTATE` pesado e postbacks completos ou MVC com Razor basico. | Blazor com componentes reativos `.razor`, DOM Virtual, zero ViewState e suporte a WebAssembly e Server. |
| **Tempo Real** | SignalR legado dependente de Newtonsoft.Json e transporte HTTP polling fragil. | SignalR moderno integrado sobre Kestrel, WebSocket de alta velocidade e serializacao binaria MessagePack. |

---

## 4. Inversao de Controle (IoC) e Configuracao

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Injecao de Dependencias** | Inexistente na biblioteca base; dependia de Service Locator (`DependencyResolver`) e frameworks externos como Unity ou Ninject. | Container IoC nativo e ultrarrapido integrado desde o `Program.cs` com ciclos de vida Transient, Scoped e Singleton. |
| **Sistema de Configuracao** | Arquivos estaticos XML `Web.config` e `App.config`, cuja alteracao causava reciclagem do AppPool no IIS. | Provedores multiplos e composiveis (`appsettings.json`, variaveis de ambiente, secrets, CLI) com reload a quente. |
| **Tipagem de Configuracoes** | `ConfigurationManager.AppSettings["chave"]` retornando strings soltas sem validacao. | Options Pattern tipado (`IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`) com validacao DataAnnotations. |

---

## 5. Acesso a Dados e Persistencia

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Entity Framework** | EF 6 classico com geracao de SQL verboso, inicializacao fria lenta e metodos assincronos parciais. | EF Core com traducao de consultas otimizada, Compiled Queries, Split Queries e Interceptors. |
| **Connection Pooling** | Padrao classico gerenciado por driver ADO.NET estatico no Windows. | Pooling de conexoes de alta performance com DbContext Pooling (`AddDbContextPool`). |
| **Rastreamento de Entidades** | Sobrecarregava a memoria com snapshots de objetos pesados sem opcao de desativacao facil. | `AsNoTracking()` e `AsNoTrackingWithIdentityResolution()` para consultas somente leitura com consumo minimo de heap. |

---

## 6. Programacao Assincrona e Paralelismo

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Contexto de Sincronizacao** | `AspNetSynchronizationContext` propenso a deadlocks criticos ao chamar `.Result` ou `.Wait()`. | Sem SynchronizationContext no ASP.NET Core; execucao direta no ThreadPool gerenciado. |
| **Tipos de Retorno Assincronos** | Exclusivamente `Task` e `Task<T>`, forçando alocacao no heap mesmo para conclusoes sincronas. | `ValueTask` e `ValueTask<T>` para retorno zero-allocation em caminhos ja concluidos em memoria. |
| **Streaming de Dados** | Retornar colecoes exigia carregar tudo na memoria (`Task<List<T>>`) antes de enviar o primeiro byte. | `IAsyncEnumerable<T>` e `await foreach` para streaming continuo de itens direto do banco para o cliente HTTP. |

---

## 7. Desempenho, Memoria e Serializacao

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Manipulacao de Memoria** | Substrings e copias frequentes de `byte[]`, causando alta pressao no Garbage Collector. | `Span<T>`, `ReadOnlySpan<T>` e `Memory<T>` para fatiamento de arrays sem nenhuma alocacao no heap. |
| **Reaproveitamento de Buffers** | Nao padronizado; criacao e destruicao continua de buffers de I/O. | `ArrayPool<T>.Shared` para aluguel e devolucao de buffers de alta capacidade. |
| **Serializacao JSON** | `Newtonsoft.Json` com uso massivo de reflexao dinâmica e alocacoes intermediarias. | `System.Text.Json` com Source Generators (`JsonSerializerContext`), leitura em UTF-8 direto e zero reflexao. |
| **Estrategias de Cache** | MemoryCache isolado sem protecao a Cache Stampede e sem invalidacao por tags. | `HybridCache` com duas camadas unificadas (L1/L2), bloqueio concorrente atomico e `RemoveByTagAsync`. |

---

## 8. Observabilidade e Diagnosticos

| Dimensao | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Rastreamento Distribuido** | Nenhum; dependia de IDs manuais em headers HTTP sem arvore hierarquica de spans. | Padrão W3C Trace Context (`traceparent`) nativo via `System.Diagnostics.ActivitySource`. |
| **Metricas de Servico** | PerfMon do Windows com contadores acoplados ao Registro do SO. | `System.Diagnostics.Metrics` com instrumentos exportaveis via protocolo OTLP para Prometheus e Aspire. |
| **Tratamento de Falhas** | `customErrors` no Web.config e `Application_Error` no Global.asax com HTML generico. | `IExceptionHandler` e conformidade internacional com o padrao RFC 7807/9457 ProblemDetails. |
| **Health Checks** | Paginas `.ashx` rudimentares ou scripts de ping sem padronizacao. | `AddHealthChecks` com separacao entre probes de Liveness e Readiness para orquestradores Kubernetes. |
