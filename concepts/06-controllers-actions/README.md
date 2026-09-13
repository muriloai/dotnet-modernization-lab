# Lab 06: Controllers e Actions

> Comparativo prático entre a divisão de controllers no **.NET Framework 4.8.1** (`System.Web.Mvc.Controller` com `ViewResult`/`JsonResult` vs `System.Web.Http.ApiController` com `IHttpActionResult`, validação manual repetitiva e `JsonRequestBehavior`) e o modelo unificado do **.NET 10** (`ControllerBase`, atributo `[ApiController]` com geração automática de `ProblemDetails`, `TypedResults` e paridade com Minimal APIs).

---

## O que é este conceito?

Os **Controllers e Actions** representam a camada de controle responsável por receber requisições HTTP, executar o binding de parâmetros, acionar os serviços de negócio e devolver uma resposta adequada (seja um documento HTML renderizado ou um payload JSON estruturado).

- No **.NET Framework (1.0 até 4.8.1)**, os controllers eram divididos em dois mundos totalmente separados:
  - Os controllers de tela herdavam de `System.Web.Mvc.Controller`, retornando `ActionResult` (`View()`, `PartialView()`, `JsonResult`). Ao devolver JSON em requisições GET, era obrigatório passar o argumento `JsonRequestBehavior.AllowGet`.
  - Os controllers de API REST herdavam de `System.Web.Http.ApiController`, retornando `IHttpActionResult` (`Ok()`, `NotFound()`, `BadRequest()`).
  - Em ambos, a validação de modelos exigia a repetição manual de blocos de verificação como `if (!ModelState.IsValid) return BadRequest(ModelState);` em todas as ações de escrita.
- No **.NET 10 (e no ASP.NET Core unificado)**, existe uma única hierarquia limpa:
  - APIs herdam de `ControllerBase`, que fornece métodos auxiliares sem carregar a sobrecarga desnecessária do mecanismo de Views.
  - O atributo `[ApiController]` habilita comportamentos automatizados essenciais: inferência automática de fontes de binding (`[FromBody]`, `[FromRoute]`, `[FromQuery]`) e resposta automática com código 400 no formato padronizado **ProblemDetails (RFC 7807)** se o modelo for inválido.
  - Resultados fortemente tipados com `ActionResult<T>` e `TypedResults` melhoram a testabilidade de unidade e a documentação via OpenAPI/Swagger.

---

## Por que mudou?

1. **Unificação da Base de Código:** No ecossistema antigo, métodos auxiliares de mesmo nome (como `Ok()` ou `NotFound()`) existiam em namespaces diferentes com assinaturas e comportamentos distintos. O .NET moderno unificou tudo sob `Microsoft.AspNetCore.Mvc`.
2. **Eliminação do Boilerplate de Validação:** A exigência de escrever `if (!ModelState.IsValid)` em cada action de criação ou edição abria espaço para esquecimentos e respostas inconsistentes. O atributo `[ApiController]` intercepta a requisição antes da action se os dados forem inválidos.
3. **Padronização de Erros com ProblemDetails (RFC 7807):** Em vez de retornar formatos proprietários de erro, o .NET moderno devolve um padrão de mercado universal com campos como `type`, `title`, `status` e `errors`.
4. **Fim do `JsonRequestBehavior.AllowGet`:** No legado, o ASP.NET MVC bloqueava retornos JSON em verbos GET por padrão para prevenir vulnerabilidades de JSON Hijacking que afetavam navegadores antigos dos anos 2000. No .NET moderno, isso não é mais necessário.
5. **Tipagem Estrita com `TypedResults` e `ActionResult<T>`:** No legado, controllers devolviam interfaces fracamente tipadas (`IHttpActionResult`), exigindo conversões manuais em testes unitários. No .NET 10, o tipo exato do retorno pode ser inferido pelo compilador.

---

## Comparativo Direto

| Dimensão Técnica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Classe Base para APIs** | `System.Web.Http.ApiController` | `Microsoft.AspNetCore.Mvc.ControllerBase` |
| **Classe Base para Views** | `System.Web.Mvc.Controller` | `Microsoft.AspNetCore.Mvc.Controller` (herda de `ControllerBase`) |
| **Retorno de JSON** | `return Json(dados, JsonRequestBehavior.AllowGet)` no MVC ou `Ok(dados)` na Web API | `return Ok(dados)` ou `TypedResults.Ok(dados)` |
| **Validação de Modelo** | Manual em cada action: `if (!ModelState.IsValid) return BadRequest(...)` | Automática via `[ApiController]`, retornando 400 ProblemDetails |
| **Formato de Erro Padrão** | Dicionário serializado do `ModelStateDictionary` | Padrão RFC 7807 **ProblemDetails** estruturado |
| **Inferência de Binding** | Manual com `[FromBody]` e `[FromUri]` | Automática e inteligente pelo `[ApiController]` |
| **Paridade com Minimal APIs** | Inexistente (código de controller não se integra a endpoints diretos) | Total: `TypedResults` pode ser retornado tanto em controllers quanto em Minimal APIs |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── ControllersAndActionsDemo.csproj     # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Pacotes NuGet: MVC 5, Web API 2 e Newtonsoft.Json
├── Web.config                          # Configuração de handlers e assemblies
├── Global.asax                         # Inicialização da aplicação
├── Global.asax.cs                      # Registro de rotas MVC e Web API
├── App_Start/
│   ├── RouteConfig.cs                  # Roteamento do MVC
│   └── WebApiConfig.cs                 # Roteamento do Web API
├── Models/
│   ├── ProdutoDto.cs                   # DTO para transferência via Web API
│   └── ProdutoViewModel.cs             # ViewModel para formulários MVC
├── Controllers/
│   ├── ProdutosMvcController.cs        # Controller herdando de System.Web.Mvc.Controller
│   └── Api/
│       └── ProdutosApiController.cs    # Controller herdando de System.Web.Http.ApiController
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config
│   ├── Shared/_Layout.cshtml
│   └── ProdutosMvc/
│       ├── Index.cshtml                # Listagem e teste de JsonResult
│       └── Criar.cshtml                # Formulário MVC com validação manual
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── ControllersAndActionsDemo.csproj    # SDK-style minimalista visando net10.0
├── Program.cs                          # Registro de AddControllers, MapControllers e Minimal API
├── Models/
│   ├── ProdutoDto.cs                   # Record de produto
│   └── CriarProdutoRequest.cs          # Modelo com DataAnnotations para validação automática
├── Controllers/
│   └── ProdutosController.cs           # Controller herdando de ControllerBase com [ApiController]
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5600
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)
Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/06-controllers-actions/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5600`).

Endpoints didáticos disponíveis:
- `GET /`: Painel visual didático que permite testar envio de dados válidos e dados inválidos para observar a resposta automática do `ProblemDetails`.
- `GET /api/produtos`: Lista todos os produtos via `ProdutosController` (`ControllerBase`).
- `GET /api/produtos/1`: Obtém produto por ID com `ActionResult<ProdutoDto>`.
- `POST /api/produtos`: Criação de produto com validação automática.
- `GET /api/minimal/produtos`: Endpoint equivalente implementado via Minimal API com `TypedResults.Ok()`.

**Teste de Validação Automática com ProblemDetails:**
Envie uma requisição POST para `/api/produtos` com payload inválido (nome vazio e preço negativo):
```json
{
  "nome": "",
  "categoria": "hardware",
  "preco": -50.00
}
```
Observe a resposta com status HTTP 400 no formato RFC 7807 sem que nenhuma linha de validação manual tenha sido escrita dentro da action do controller.

---

### Versão Legada (.NET Framework 4.8.1)
1. Abra o projeto `ControllersAndActionsDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse:
   - `http://localhost:5601/`: View MVC servida por `ProdutosMvcController`.
   - `http://localhost:5601/produtosmvc/criar`: Formulário de criação MVC.
   - `http://localhost:5601/produtosmvc/obterjson`: Demonstração da obrigatoriedade do `JsonRequestBehavior.AllowGet`.
   - `http://localhost:5601/api/produtos`: Endpoint Web API servido por `ProdutosApiController`.

Para compilar apenas via terminal (requer MSBuild do Windows):
```cmd
msbuild concepts\06-controllers-actions\framework\ControllersAndActionsDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A ausência de `if (!ModelState.IsValid)` no .NET 10:** No controller `ProdutosController.cs` do .NET 10, a action `Criar` nem sequer verifica o estado do modelo; o framework rejeita a requisição antes de invocá-la caso as anotações do `CriarProdutoRequest` sejam violadas.
2. **O formato da resposta de erro:** Compare o JSON retornado pelo Framework em caso de erro de validação contra o formato padronizado **ProblemDetails** retornado pelo .NET 10.
3. **Paridade entre Controllers e Minimal APIs:** Veja no `Program.cs` do .NET 10 como os mesmos tipos de retorno (`TypedResults`) podem ser utilizados tanto em controllers quanto em endpoints diretos.
