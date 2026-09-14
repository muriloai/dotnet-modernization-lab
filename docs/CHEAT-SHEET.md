# Guia Rapido de Referencia: De .NET Framework 4.8.1 Para .NET 10

Este guia pratico serve como um dicionario de traducao rapida ("De -> Para") para engenheiros de software migrando bases de codigo do .NET Framework 4.8.1 para o .NET 10.

---

## 1. Sistema e Linha de Comando

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Compilar projeto** | `msbuild MeuProjeto.csproj /p:Configuration=Release` | `dotnet build -c Release` |
| **Executar aplicacao** | Iniciar pelo Visual Studio no IIS Express | `dotnet run` |
| **Publicar aplicacao** | `msdeploy.exe` ou Publicacao pelo Visual Studio | `dotnet publish -c Release -o ./publish` |
| **Executar testes** | `vstest.console.exe MeuProjeto.Tests.dll` | `dotnet test` |
| **Adicionar pacote** | `Install-Package Pacote` no Package Manager Console | `dotnet add package Pacote` |

---

## 2. Configuracao da Aplicacao

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Ler string simples** | `ConfigurationManager.AppSettings["MinhaChave"]` | `builder.Configuration["MinhaChave"]` |
| **Ler Connection String** | `ConfigurationManager.ConnectionStrings["Sql"].ConnectionString` | `builder.Configuration.GetConnectionString("Sql")` |
| **Mapear objeto tipado** | Criar classe manual com `ConfigurationSection` | `builder.Services.Configure<MinhaConfig>(builder.Configuration.GetSection("MinhaConfig"))` |
| **Consumir configuracao** | Acesso estatico `ConfigurationManager` | Injetar `IOptions<MinhaConfig>` ou `IOptionsSnapshot<MinhaConfig>` |

---

## 3. Injecao de Dependencias (IoC)

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Configurar container** | Instalar Unity, Autofac ou Ninject e configurar `DependencyResolver` | Nativo: `builder.Services.AddTransient`, `AddScoped`, `AddSingleton` |
| **Instancia por requisicao** | Ciclo de vida personalizado dependente de `HttpContext` | `builder.Services.AddScoped<IServico, Servico>()` |
| **Instancia unica global** | Variavel estatica `static readonly` ou Singleton manual | `builder.Services.AddSingleton<IServico, Servico>()` |
| **Resolver dependencia** | `DependencyResolver.Current.GetService<IServico>()` (Service Locator) | Injetar diretamente no construtor da classe ou endpoint Minimal API |

---

## 4. Pipeline e Interceptacao de Requisicoes

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Interceptar requisicoes** | Criar classe herdando de `IHttpModule` e registrar no `Web.config` | Criar Middleware customizado ou usar `app.Use(async (ctx, next) => ...)` |
| **Finalizar pipeline** | `HttpHandler` herdando de `IHttpHandler` | `app.MapGet` ou Middleware terminal |
| **Acessar dados da requisicao** | `HttpContext.Current.Request` | Injetar `HttpContext` diretamente no metodo do endpoint |
| **Tratamento de erros global** | Metodo `Application_Error` no `Global.asax.cs` | `app.UseExceptionHandler()` e implementar `IExceptionHandler` |

---

## 5. APIs e Roteamento

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Definir rota basica** | Rota MVC ou Web API herdando de `ApiController` | `app.MapGet("/api/itens", () => ...)` |
| **Agrupamento de rotas** | Definir atributos de rota repetidos em cada controller | `var grupo = app.MapGroup("/api/v1/itens");` |
| **Retornar dados JSON** | `return Request.CreateResponse(HttpStatusCode.OK, model);` | `return TypedResults.Ok(model);` |
| **Retornar erro tipado** | `return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "erro");` | `return TypedResults.Problem(detail: "erro", statusCode: 400);` |

---

## 6. Acesso a Banco de Dados

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Registrar DbContext** | Instanciar `new MeuDbContext()` manualmente ou via factory | `builder.Services.AddDbContext<MeuDbContext>(opt => opt.UseSqlServer(...))` |
| **Consulta somente leitura** | `context.Clientes.AsNoTracking().ToList()` | `await context.Clientes.AsNoTracking().ToListAsync()` |
| **Streaming assincrono** | Inexistente; obrigado a carregar lista em memoria | `await foreach (var item in context.Clientes.AsAsyncEnumerable())` |
| **Otimizacao de conexoes** | Padrao ADO.NET estatico | `builder.Services.AddDbContextPool<MeuDbContext>(...)` |

---

## 7. Chamadas HTTP Externas

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Requisicao HTTP simples** | `new WebClient().DownloadString(url)` ou `new HttpClient()` solto | `IHttpClientFactory` com `AddHttpClient` tipado |
| **Evitar esgotamento de sockets**| Gerenciar Singleton estatico de `HttpClient` manualmente | Injetar cliente tipado criado por `IHttpClientFactory` |
| **Resiliencia e Retry** | Codificar lacos `while/try-catch` manuais | `AddStandardResilienceHandler()` integrado ao Polly nativo |

---

## 8. Caching e Serializacao

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Armazenar em memoria** | `MemoryCache.Default.Set(chave, valor, policy)` | `await hybridCache.GetOrCreateAsync(chave, factory, tags)` |
| **Evitar Cache Stampede** | Escrever locks manuais (`lock` ou `SemaphoreSlim`) | Nativo: `HybridCache` bloqueia simultaneidade por chave |
| **Invalidar por categoria** | Varrer todas as chaves do cache com LINQ | `await hybridCache.RemoveByTagAsync("categoria-x")` |
| **Serializar JSON** | `JsonConvert.SerializeObject(obj)` (Newtonsoft.Json) | `JsonSerializer.Serialize(obj, MeuJsonContext.Default.MeuTipo)` |

---

## 9. Observabilidade e Logging

| Tarefa | No .NET Framework 4.8.1 | No .NET 10 |
| :--- | :--- | :--- |
| **Gravar log simples** | `Trace.WriteLine("mensagem")` ou `LogManager.GetLogger()` | Injetar `ILogger<T>` e chamar `logger.LogInformation(...)` |
| **Logs estruturados** | Concatenacao de strings: `logger.Info("Id: " + id)` | Template nomeado: `logger.LogInformation("Pedido {PedidoId}", id)` |
| **Rastreamento distribuido** | Criar e repassar `X-Correlation-ID` manualmente | `ActivitySource` nativo propagando W3C `traceparent` |
| **Health Checks** | Criar endpoint `.ashx` respondendo "OK" | `app.MapHealthChecks("/healthz")` |
