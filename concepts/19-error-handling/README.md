# Conceito 19: Logging e Observabilidade - Tratamento de Erros

Este laboratório analisa a evolução arquitetural no tratamento de falhas e exceções entre o ecossistema clássico do **.NET Framework 4.8.1** e a abordagem moderna do **.NET 10**.

---

## 1. Contexto e Motivação

No desenvolvimento tradicional com .NET Framework, o tratamento global de exceções sofria de grave fragmentação:
1. **Múltiplas camadas desconectadas:** Exceções podiam ser capturadas no evento `Application_Error` do `Global.asax.cs`, interceptadas pelo filtro `HandleErrorAttribute` do ASP.NET MVC, ou tratadas via `IExceptionHandler` e `IExceptionFilter` do ASP.NET Web API 2. Cada uma dessas ferramentas operava com ciclos de vida diferentes e contratos incompatíveis.
2. **Conflito de configurações no `Web.config`:** A diretiva `<customErrors mode="On">` do ASP.NET frequentemente entrava em conflito com o módulo `<httpErrors>` do IIS, gerando páginas de erro duplicadas ou substituindo respostas de APIs por páginas HTML genéricas do servidor.
3. **Ausência de padronização em APIs:** Cada aplicação inventava sua própria estrutura de resposta JSON para erros (como `{ "erro": true, "msg": "..." }`) ou retornava textos planos e códigos de status arbitrários, dificultando o consumo por clientes externos.
4. **Vazamento de informações sensíveis:** Caso o `customErrors` estivesse desativado (`mode="Off"` ou má configuração de ambiente), páginas amareladas com stack traces completos eram exibidas publicamente.

No **.NET 10**, o tratamento de erros foi unificado em um pipeline declarativo, seguro e padronizado:
- **Pipeline Unificado de Exceções:** O middleware `app.UseExceptionHandler()` captura todas as exceções não tratadas disparadas durante a execução da requisição HTTP, independentemente de terem ocorrido em controllers, Minimal APIs ou outros middlewares.
- **Padrão `IExceptionHandler`:** Introduzido a partir do .NET 8 e consolidado no .NET 10, permite registrar manipuladores especializados resolvidos diretamente pelo contêiner de injeção de dependências. Vários handlers podem ser encadeados (Chain of Responsibility), onde cada um decide se trata a falha ou a repassa para o próximo manipulador.
- **Padronização Universal via RFC 7807 e RFC 9457 (`ProblemDetails`):** O método `services.AddProblemDetails()` ativa respostas de erro padronizadas pelo IETF contendo `type`, `title`, `status`, `detail`, `instance` e extensões customizadas para correlação de diagnóstico.
- **Ambiente de Desenvolvimento Rico:** `app.UseDeveloperExceptionPage()` fornece informações detalhadas e seguras para depuração local sem risco de vazamento involuntário em produção.

---

## 2. Comparativo Técnico Direto

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Ponto Central de Captura** | Fragmentado (`Application_Error`, `HandleErrorAttribute`, `IExceptionHandler` do Web API) | Unificado via middleware `app.UseExceptionHandler()` |
| **Injeção de Dependências** | Limitada ou inexistente em filtros e eventos de ciclo de vida | Nativa; handlers recebem serviços registrados no contêiner |
| **Manipuladores Customizados** | Classes legadas de filtro ou métodos em `Global.asax` | Implementação limpa da interface `IExceptionHandler` |
| **Encadeamento de Handlers** | Complexo e propenso a conflitos entre módulos HTTP | Suporte nativo a múltiplos handlers em cadeia ordenada |
| **Formato de Resposta de Erro** | Proprietário ou páginas HTML estáticas do IIS | Padronizado conforme a especificação RFC 7807/9457 (`ProblemDetails`) |
| **Configuração de Ambiente** | XML no `Web.config` via `<customErrors>` e `<httpErrors>` | Código C# declarativo (`app.Environment.IsDevelopment()`) |
| **Rastreabilidade e Diagnóstico** | Logs manuais em arquivos ou `Trace.TraceError` | Propriedades automáticas de correlação como `TraceId` integradas |

---

## 3. O Modelo Legado (.NET Framework 4.8.1)

No .NET Framework, capturar uma falha globalmente exigia manipular o estado estático de `HttpContext.Current` no `Global.asax.cs`:

```csharp
protected void Application_Error(object sender, EventArgs e)
{
    Exception excecao = Server.GetLastError();
    
    // Tentativa manual de registrar o erro
    Trace.TraceError("Falha não tratada capturada no Application_Error: " + excecao.Message);
    
    // Limpar o erro para evitar a tela padrão do ASP.NET
    Server.ClearError();
    
    // Redirecionamento forçado para view de erro
    Response.Redirect("~/Home/Error");
}
```

No `Web.config`, era necessário gerenciar diretivas de erro:

```xml
<system.web>
  <customErrors mode="On" defaultRedirect="~/Home/Error">
    <error statusCode="404" redirect="~/Home/NotFound" />
    <error statusCode="500" redirect="~/Home/Error" />
  </customErrors>
</system.web>
```

Limitações deste formato:
1. APIs REST que retornavam exceções acabavam recebendo o HTML do redirecionamento em vez de uma resposta JSON estruturada.
2. Não havia como inspecionar facilmente a exceção original na tela de destino sem persistir temporariamente em sessão ou passar parâmetros inseguros pela URL.
3. Tratamentos de domínio (como validação de estoque ou cliente bloqueado) eram capturados e misturados com falhas catastróficas de infraestrutura.

---

## 4. O Modelo Moderno (.NET 10)

No .NET 10, manipuladores especializados implementam a interface `IExceptionHandler`.

### Manipulador de Regra de Negócio com `ProblemDetails`
```csharp
public class RegraNegocioExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public RegraNegocioExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RegraNegocioException regraEx)
        {
            return false; // Repassa para o próximo handler na cadeia
        }

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Regra de negócio violada",
                Detail = regraEx.Message,
                Type = "https://erros.exemplo.com/regras-de-negocio"
            }
        });
    }
}
```

### Registro no Pipeline Moderno (`Program.cs`)
```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

// Cadeia de responsabilidade de handlers
builder.Services.AddExceptionHandler<EntidadeNaoEncontradaExceptionHandler>();
builder.Services.AddExceptionHandler<RegraNegocioExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler(); // Ativa o pipeline unificado
```

Benefícios diretos:
1. **Controllers limpos:** Actions focam estritamente no caso de sucesso e lançam exceções de domínio expressivas sem a necessidade de blocos `try-catch` em cada método.
2. **Respostas padronizadas:** Clientes recebem payloads previsíveis no formato `application/problem+json` reconhecido universalmente.
3. **Segurança:** Detalhes internos de infraestrutura nunca são expostos em ambientes de produção.

---

## 5. Estrutura deste Laboratório

```text
concepts/19-error-handling/
├── README.md
├── framework/
│   ├── App_Start/
│   │   ├── FilterConfig.cs
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Exceptions/
│   │   └── RegraNegocioLegadaException.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   ├── Error.cshtml
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── ErrorHandlingDemo.csproj
│   ├── packages.config
│   └── Web.config
└── net10/
    ├── Controllers/
    │   └── ProdutosController.cs
    ├── Exceptions/
    │   ├── EntidadeNaoEncontradaException.cs
    │   ├── IntegracaoExternaException.cs
    │   └── RegraNegocioException.cs
    ├── Handlers/
    │   ├── EntidadeNaoEncontradaExceptionHandler.cs
    │   ├── GlobalExceptionHandler.cs
    │   └── RegraNegocioExceptionHandler.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── wwwroot/
    │   ├── css/
    │   │   └── site.css
    │   └── index.html
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── ErrorHandlingDemo.csproj
    └── Program.cs
```

---

## 6. Como Executar os Projetos

### Executando o Projeto Moderno (.NET 10)
1. Abra o terminal na pasta `concepts/19-error-handling/net10`.
2. Execute o comando:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo em `http://localhost:6900`.
4. Dispare os diferentes botões de teste para observar como cada tipo de exceção é interceptado pelo handler correspondente e transformado em um objeto RFC 7807 `ProblemDetails`.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra a pasta `concepts/19-error-handling/framework` no Visual Studio 2022 em um ambiente Windows com carga de trabalho ASP.NET.
2. Inicie o projeto através do IIS Express na porta `6901`.
3. Acesse `http://localhost:6901` para testar os formulários e visualizar como o `HandleErrorAttribute` e o `Application_Error` realizam redirecionamentos clássicos.
