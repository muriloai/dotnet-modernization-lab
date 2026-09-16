# Resumo Consolidado das Mudanças de Modernização (.NET 4.8.1 para .NET 10)

Este documento sintetiza os saltos tecnológicos e arquiteturais que ocorreram entre o **.NET Framework 4.8.1** (a versão final do framework Windows histórico) e o **.NET 10** (a versão moderna de altíssima performance, cross-platform e cloud-native).

---

## 1. Sistema de Projetos e Ferramentas de Build

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Formato de Projeto** | MSBuild clássico prolixo (centenas de linhas com GUIDs, caminhos absolutos e listagem manual de cada arquivo `.cs`). | SDK-Style limpo e declarativo (`<Project Sdk="Microsoft.NET.Sdk.Web">`), com inclusão automática de arquivos. |
| **Gerenciamento de Pacotes** | `packages.config` com pasta `packages/` local pesada ou `PackageReference` retroajustado sem suporte total. | `PackageReference` nativo e centralizado, com transitividade de dependências e cache global no usuário. |
| **CLI e Linha de Comando** | Fragmentada (`msbuild.exe`, `nuget.exe`, `vstest.console.exe`). | CLI unificada e multiplataforma através do utilitário `dotnet` (`build`, `run`, `test`, `publish`, `tool`). |
| **Multi-Targeting** | Inviável nativamente sem hacks no arquivo `.csproj`. | Suporte nativo e simplificado via tag `<TargetFrameworks>`. |

---

## 2. Modelo de Hospedagem e Runtime

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Servidor Web** | Acoplamento quase total com o Internet Information Services (IIS) e processo `w3wp.exe`. | Servidor Kestrel embutido, assíncrono, com suporte nativo a HTTP/1.1, HTTP/2, HTTP/3 e gRPC. |
| **Sistemas Operacionais** | Exclusivo do Microsoft Windows. | 100% compatível com Linux, macOS e Windows. |
| **Tamanho de Imagens de Container** | Imagens Windows Server de 6 GB a 12 GB, com boot de minutos. | Imagens Linux Chiseled e Alpine de 30 MB a 120 MB, com boot em milissegundos. |
| **Compilação Nativa** | JIT em tempo de execução com alto cold start; NGen frágil preso à máquina local. | Native AOT (`<PublishAot>`), gerando binários nativos sem necessidade de runtime CLR instalado. |

---

## 3. Arquitetura da Web e APIs

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Pipeline HTTP** | `IHttpModule` e `IHttpHandler` acoplados ao ciclo de eventos síncrono de 23 estágios do IIS. | Pipeline encadeado de Middlewares bidirecionais assíncronos baseados em `RequestDelegate`. |
| **Construção de APIs** | ASP.NET Web API 2 com herança obrigatória de `ApiController` e sobrecarga de alocação de controllers. | Minimal APIs de alto rendimento (`app.MapGet`), Route Groups, Endpoint Filters e TypedResults. |
| **Interfaces de Usuário** | ASP.NET Web Forms com `__VIEWSTATE` pesado e postbacks completos ou MVC com Razor básico. | Blazor com componentes reativos `.razor`, DOM Virtual, zero ViewState e suporte a WebAssembly e Server. |
| **Tempo Real** | SignalR legado dependente de Newtonsoft.Json e transporte HTTP polling frágil. | SignalR moderno integrado sobre Kestrel, WebSocket de alta velocidade e serialização binária MessagePack. |

---

## 4. Inversão de Controle (IoC) e Configuração

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Injeção de Dependências** | Inexistente na biblioteca base; dependia de Service Locator (`DependencyResolver`) e frameworks externos como Unity ou Ninject. | Container IoC nativo e ultrarrápido integrado desde o `Program.cs` com ciclos de vida Transient, Scoped e Singleton. |
| **Sistema de Configuração** | Arquivos estáticos XML `Web.config` e `App.config`, cuja alteração causava reciclagem do AppPool no IIS. | Provedores múltiplos e composíveis (`appsettings.json`, variáveis de ambiente, secrets, CLI) com reload a quente. |
| **Tipagem de Configurações** | `ConfigurationManager.AppSettings["chave"]` retornando strings soltas sem validação. | Options Pattern tipado (`IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`) com validação DataAnnotations. |

---

## 5. Acesso a Dados e Persistência

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Entity Framework** | EF 6 clássico com geração de SQL verboso, inicialização fria lenta e métodos assíncronos parciais. | EF Core com tradução de consultas otimizada, Compiled Queries, Split Queries e Interceptors. |
| **Connection Pooling** | Padrão clássico gerenciado por driver ADO.NET estático no Windows. | Pooling de conexões de alta performance com DbContext Pooling (`AddDbContextPool`). |
| **Rastreamento de Entidades** | Sobrecarregava a memória com snapshots de objetos pesados sem opção de desativação fácil. | `AsNoTracking()` e `AsNoTrackingWithIdentityResolution()` para consultas somente leitura com consumo mínimo de heap. |

---

## 6. Programação Assíncrona e Paralelismo

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Contexto de Sincronização** | `AspNetSynchronizationContext` propenso a deadlocks críticos ao chamar `.Result` ou `.Wait()`. | Sem SynchronizationContext no ASP.NET Core; execução direta no ThreadPool gerenciado. |
| **Tipos de Retorno Assíncronos** | Exclusivamente `Task` e `Task<T>`, forçando alocação no heap mesmo para conclusões síncronas. | `ValueTask` e `ValueTask<T>` para retorno zero-allocation em caminhos já concluídos em memória. |
| **Streaming de Dados** | Retornar coleções exigia carregar tudo na memória (`Task<List<T>>`) antes de enviar o primeiro byte. | `IAsyncEnumerable<T>` e `await foreach` para streaming contínuo de itens direto do banco para o cliente HTTP. |

---

## 7. Desempenho, Memória e Serialização

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Manipulação de Memória** | Substrings e cópias frequentes de `byte[]`, causando alta pressão no Garbage Collector. | `Span<T>`, `ReadOnlySpan<T>` e `Memory<T>` para fatiamento de arrays sem nenhuma alocação no heap. |
| **Reaproveitamento de Buffers** | Não padronizado; criação e destruição contínua de buffers de I/O. | `ArrayPool<T>.Shared` para aluguel e devolução de buffers de alta capacidade. |
| **Serialização JSON** | `Newtonsoft.Json` com uso massivo de reflexão dinâmica e alocações intermediárias. | `System.Text.Json` com Source Generators (`JsonSerializerContext`), leitura em UTF-8 direto e zero reflexão. |
| **Estratégias de Cache** | MemoryCache isolado sem proteção contra Cache Stampede e sem invalidação por tags. | `HybridCache` com duas camadas unificadas (L1/L2), bloqueio concorrente atômico e `RemoveByTagAsync`. |

---

## 8. Observabilidade e Diagnósticos

| Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Rastreamento Distribuído** | Nenhum; dependia de IDs manuais em headers HTTP sem árvore hierárquica de spans. | Padrão W3C Trace Context (`traceparent`) nativo via `System.Diagnostics.ActivitySource`. |
| **Métricas de Serviço** | PerfMon do Windows com contadores acoplados ao Registro do SO. | `System.Diagnostics.Metrics` com instrumentos exportáveis via protocolo OTLP para Prometheus e Aspire. |
| **Tratamento de Falhas** | `customErrors` no Web.config e `Application_Error` no Global.asax com HTML genérico. | `IExceptionHandler` e conformidade internacional com o padrão RFC 7807/9457 ProblemDetails. |
| **Health Checks** | Páginas `.ashx` rudimentares ou scripts de ping sem padronização. | `AddHealthChecks` com separação entre probes de Liveness e Readiness para orquestradores Kubernetes. |
