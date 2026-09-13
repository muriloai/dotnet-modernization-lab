# Conceito 14: Construção de APIs REST

Este laboratório compara a arquitetura e os padrões de desenvolvimento de APIs REST no **.NET Framework 4.8.1 (ASP.NET Web API 2)** com o modelo moderno no **.NET 10 (ASP.NET Core Web API e Minimal APIs)**.

---

## 1. Visão Geral e Evolução

No .NET Framework 4.8.1, o desenvolvimento de serviços HTTP era segregado entre WCF (SOAP) e ASP.NET Web API 2. A Web API 2 introduziu conceitos como `ApiController` e `IHttpActionResult`, mas dependia fortemente do pipeline do IIS (`System.Web`), exigia containers de injeção de dependência externos acoplados a `IDependencyResolver` e não possuía suporte nativo para especificações modernas de documentação e padronização de erros.

No .NET 10, a construção de APIs HTTP é unificada e ultraleve:
- A base é o `ControllerBase` com o atributo `[ApiController]`, eliminando a separação artificial entre MVC e Web API.
- O runtime oferece suporte nativo a respostas tipadas via `ActionResult<T>` e `TypedResults`, padronização automática de erros com RFC 7807 (`ProblemDetails`) e geração de contratos OpenAPI v3 diretamente no pipeline via `Microsoft.AspNetCore.OpenApi`.
- Além dos controllers convencionais, o .NET 10 oferece **Minimal APIs**, permitindo declarar endpoints com alta performance de alocação zero de memória e sintaxe funcional expressiva.

---

## 2. Comparativo Técnico: Legado vs Moderno

| Característica | ASP.NET Web API 2 (.NET Framework 4.8.1) | ASP.NET Core Web API (.NET 10) |
| :--- | :--- | :--- |
| **Classe Base** | `System.Web.Http.ApiController` | `Microsoft.AspNetCore.Mvc.ControllerBase` |
| **Atributo de API** | Não existia (inferido pela herança) | `[ApiController]` (ativa inferência de fontes e validação) |
| **Injeção de Dependências** | Não embutida (`IDependencyResolver` manual) | Nativa no container (`AddScoped`, `AddSingleton`, etc.) |
| **Tipagem de Retorno** | `HttpResponseMessage` ou `IHttpActionResult` | `ActionResult<T>` ou `TypedResults` fortemente tipados |
| **Tratamento de Validação** | Checagem manual de `if (!ModelState.IsValid)` | Automático via `[ApiController]` com RFC 7807 |
| **Padronização de Erro** | `Request.CreateErrorResponse` ad-hoc | RFC 7807 `ProblemDetails` nativo |
| **Documentação OpenAPI** | Swashbuckle 5.x legado com configuração XML | OpenAPI nativo integrado (`AddOpenApi`, `MapOpenApi`) |
| **Abordagem de Endpoints** | Exclusivamente controllers em classes | Controllers convencionais e Minimal APIs funcionais |
| **Content Negotiation** | `MediaTypeFormatterCollection` no IIS | `OutputFormatters` integrados e atributos `[Produces]` |

---

## 3. O Dilema do Retorno: IHttpActionResult vs ActionResult&lt;T&gt; e TypedResults

### No ASP.NET Web API 2:
```csharp
// Retorno opaco: a assinatura não informa o tipo do dado trafegado
[Route("{id:int}")]
public IHttpActionResult Get(int id)
{
    var produto = _repositorio.ObterPorId(id);
    if (produto == null)
    {
        return NotFound();
    }
    return Ok(produto);
}
```
A interface `IHttpActionResult` abstrai a resposta, mas esconde o tipo retornado das ferramentas de geração de clientes e documentação Swagger sem anotações adicionais (`[ResponseType(typeof(ProdutoModel))]`).

### No .NET 10 com Controllers:
```csharp
// Retorno híbrido: combina verificação de status HTTP e tipagem forte do payload
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public ActionResult<ProdutoDto> ObterPorId(int id)
{
    var produto = _produtoService.ObterPorId(id);
    if (produto is null)
    {
        return NotFound();
    }
    return Ok(produto);
}
```

### No .NET 10 com Minimal APIs:
```csharp
// Retorno estritamente tipado via TypedResults sem reflexão em tempo de execução
group.MapGet("/{id:int}", Results<Ok<ProdutoDto>, NotFound<ProblemDetails>> (int id, IProdutoService service) =>
{
    var produto = service.ObterPorId(id);
    return produto is not null
        ? TypedResults.Ok(produto)
        : TypedResults.NotFound(new ProblemDetails { Title = "Produto não encontrado", Status = 404 });
});
```

---

## 4. Estrutura do Laboratório

```text
concepts/14-rest-apis/
├── README.md
├── net10/
│   ├── Properties/launchSettings.json   (Porta Kestrel 6400)
│   ├── Models/                          (DTOs e entidades imutáveis)
│   ├── Services/                        (Regras de negócio com DI nativa)
│   ├── Controllers/ProdutosController.cs (Controller tradicional com [ApiController])
│   ├── Endpoints/ProdutosEndpoints.cs   (Mapeamento de Minimal APIs com TypedResults)
│   ├── wwwroot/                         (Dashboard interativo de testes)
│   ├── Program.cs                       (AddOpenApi, MapOpenApi, DI, Controllers e Minimal APIs)
│   └── RestApisDemo.csproj
└── framework/
    ├── Properties/AssemblyInfo.cs
    ├── App_Start/RouteConfig.cs
    ├── App_Start/WebApiConfig.cs        (Rotas de atributos da Web API 2)
    ├── Controllers/Api/ProdutosApiController.cs (ApiController legado com IHttpActionResult)
    ├── Controllers/HomeController.cs    (Interface MVC de consumo e testes)
    ├── Models/                          (Modelos de dados clássicos)
    ├── Services/                        (Repositório estático simulando persistência)
    ├── Views/Home/Index.cshtml          (Painel interativo legado)
    ├── Content/Site.css
    ├── Global.asax / Global.asax.cs
    ├── Web.config                       (Porta IIS Express 6401)
    ├── packages.config
    └── RestApisDemo.csproj
```

---

## 5. Como Executar os Projetos

### Projeto Moderno (.NET 10)
1. Navegue até o diretório `concepts/14-rest-apis/net10/`.
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```text
   http://localhost:6400
   ```
4. Inspecione o endpoint de especificação OpenAPI gerado nativamente pelo .NET 10:
   ```text
   http://localhost:6400/openapi/v1.json
   ```

### Projeto Legado (.NET Framework 4.8.1)
1. Abra o arquivo de solução ou o projeto `RestApisDemo.csproj` localizado em `concepts/14-rest-apis/framework/` no Visual Studio 2022.
2. Inicie a execução sob o IIS Express (configurado para a porta `6401`).
3. Acesse a interface no navegador:
   ```text
   http://localhost:6401
   ```
