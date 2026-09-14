# Conceito 16: Action Filters

Este laboratório compara o funcionamento, ciclo de vida e injeção de dependência em **Action Filters** no **.NET Framework 4.8.1 (ASP.NET MVC 5 e Web API 2)** com o modelo unificado e moderno no **.NET 10 (ASP.NET Core Action Filters e Endpoint Filters)**.

---

## 1. Visão Geral e Contexto Histórico

Filtros de ação (Action Filters) são interceptadores que executam código antes e depois da execução de uma Action. Eles são amplamente utilizados para auditoria, validação de regras transversais, autorização customizada e medição de desempenho.

No .NET Framework 4.8.1, existia uma grande fragmentação:
- **Duas hierarquias incompatíveis:** `System.Web.Mvc.ActionFilterAttribute` (para controllers MVC) e `System.Web.Http.Filters.ActionFilterAttribute` (para Web API 2). Se o projeto contivesse telas e APIs, os filtros precisavam ser reescritos em duplicidade.
- **Falta de injeção de dependência:** Atributos C# exigem constantes em seus construtores. Como o runtime clássico instanciava os atributos diretamente via reflexão, não era possível injetar serviços via construtor. A saída comum era o uso de Service Locator (`DependencyResolver.Current.GetService<T>()`), que tornava o código acoplado e difícil de testar.

No .NET 10, o ecossistema é unificado e integrado ao container nativo:
- **Pipeline assíncrono moderno:** `IAsyncActionFilter` utiliza `ActionExecutingContext` e um delegate assíncrono `next()`, permitindo encadeamento natural de código antes e depois da action.
- **Injeção de dependência nativa:** Através de `[ServiceFilter(typeof(MeuFiltro))]` (filtro gerenciado pelo DI) ou `[TypeFilter(typeof(MeuFiltro))]` (instanciado sob demanda com dependências resolvidas).
- **Endpoint Filters em Minimal APIs:** Funções de filtro encadeadas diretamente nas rotas com `AddEndpointFilter`, sem overhead de reflexão de atributos.

---

## 2. Comparativo Técnico: Legado vs Moderno

| Característica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Unificação** | Duas classes distintas: `System.Web.Mvc` e `System.Web.Http` | Unificado em `Microsoft.AspNetCore.Mvc.Filters` |
| **Injeção de Dependências** | Inexistente (exigia Service Locator manual) | Suporte nativo via `ServiceFilter`, `TypeFilter` ou global |
| **Execução Assíncrona** | Predominantemente síncrono com métodos separados | `IAsyncActionFilter` único com `await next()` |
| **Curto-Circuito (Short-Circuit)** | Atribuição de `actionContext.Response` manual | Atribuição de `context.Result` com `IActionResult` |
| **Suporte a Minimal APIs** | Não aplicável (modelo inexistente no legado) | `EndpointFilter` com delegates funcionais encadeados |
| **Ciclo de Vida do Filtro** | Instância única mantida em cache de atributo | Scoped, Transient ou Singleton via DI |

---

## 3. O Dilema da Injeção de Dependência nos Filtros

### No .NET Framework 4.8.1 (Anti-padrão Service Locator):
```csharp
public class ValidarApiKeyAttribute : System.Web.Http.Filters.ActionFilterAttribute
{
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        // ANTI-PADRÃO: O filtro precisa resolver serviços estaticamente
        var servico = (IApiKeyValidator)actionContext.Request.GetDependencyScope()
            .GetService(typeof(IApiKeyValidator));

        if (!servico.Validar(actionContext.Request.Headers))
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}
```

### No .NET 10 com ServiceFilter e Injeção no Construtor:
```csharp
// O filtro é uma classe normal com injeção de dependências limpa no construtor
public sealed class ValidarApiKeyFilter : IAsyncActionFilter
{
    private readonly IApiKeyValidator _validator;

    public ValidarApiKeyFilter(IApiKeyValidator validator)
    {
        _validator = validator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_validator.Validar(context.HttpContext.Request.Headers))
        {
            context.Result = new UnauthorizedObjectResult(new { Erro = "Chave de API inválida." });
            return; // Curto-circuito: impede a execução da action
        }

        await next(); // Executa a action
    }
}

// Aplicação no controller ou action
[ServiceFilter(typeof(ValidarApiKeyFilter))]
[HttpGet("dados-protegidos")]
public IActionResult ObterDados() => Ok(...);
```

---

## 4. Estrutura do Laboratório

```text
concepts/16-action-filters/
├── README.md
├── net10/
│   ├── Properties/launchSettings.json   (Porta Kestrel 6600)
│   ├── Filters/
│   │   ├── TempoExecucaoActionFilter.cs (Mede tempo e injeta cabeçalho X-Tempo-Execucao-Ms)
│   │   └── ValidarApiKeyServiceFilter.cs (Filtro com DI nativa para validação de chave de API)
│   ├── Services/
│   │   ├── IApiKeyValidatorService.cs
│   │   └── ApiKeyValidatorService.cs    (Serviço scoped validando chaves de integração)
│   ├── Controllers/
│   │   └── RelatoriosController.cs      (Demonstra ServiceFilter, TypeFilter e curto-circuito)
│   ├── Endpoints/
│   │   └── RelatoriosEndpoints.cs       (Demonstra EndpointFilter equivalente em Minimal API)
│   ├── wwwroot/                         (Dashboard interativo de execução e auditoria)
│   ├── Program.cs                       (Registro dos filtros e injeção de dependências)
│   └── ActionFiltersDemo.csproj
└── framework/
    ├── Properties/AssemblyInfo.cs
    ├── App_Start/RouteConfig.cs
    ├── App_Start/WebApiConfig.cs
    ├── Filters/
    │   ├── MedirTempoExecucaoFilter.cs   (Filtro Web API 2 com OnActionExecuting e OnActionExecuted)
    │   └── ValidarApiKeyLegadoAttribute.cs (Filtro Web API 2 usando Service Locator)
    ├── Controllers/Api/RelatoriosApiController.cs
    ├── Controllers/HomeController.cs
    ├── Services/
    │   └── ValidadorApiKeyEstatico.cs   (Validação estática necessária pela ausência de DI)
    ├── Views/Home/Index.cshtml          (Painel interativo legado)
    ├── Content/Site.css
    ├── Global.asax / Global.asax.cs
    ├── Web.config                       (Porta IIS Express 6601)
    ├── packages.config
    └── ActionFiltersDemo.csproj
```

---

## 5. Como Executar os Projetos

### Projeto Moderno (.NET 10)
1. Navegue até o diretório `concepts/16-action-filters/net10/`.
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```text
   http://localhost:6600
   ```
4. Teste a interceptação de requisições, injeção de cabeçalhos de auditoria de performance e curto-circuito com e sem chave de API válida.

### Projeto Legado (.NET Framework 4.8.1)
1. Abra o arquivo de solução ou o projeto `ActionFiltersDemo.csproj` localizado em `concepts/16-action-filters/framework/` no Visual Studio 2022.
2. Inicie a execução sob o IIS Express (configurado para a porta `6601`).
3. Acesse a interface no navegador:
   ```text
   http://localhost:6601
   ```
