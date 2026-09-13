# Lab 05: Roteamento

> Comparativo prático entre a divisão de rotas no **.NET Framework 4.8.1** (duas tabelas separadas para MVC e Web API, classes base distintas `Controller` vs `ApiController` e risco de conflito de precedência) e o modelo moderno do **.NET 10** (Endpoint Routing unificado, grupos de rotas com `app.MapGroup()`, restrições inline e `LinkGenerator`).

---

## O que é este conceito?

O **sistema de roteamento** é o mecanismo responsável por analisar a URL e o método HTTP de uma requisição e direcioná-la para o componente responsável por atendê-la (seja uma action de controller ou uma função de endpoint).

- No **.NET Framework (1.0 até 4.8.1)**, a pilha web era fragmentada em dois produtos distintos: o **ASP.NET MVC** (para páginas HTML) e o **ASP.NET Web API** (para serviços REST JSON). Cada um possuía sua própria tabela de rotas (`RouteTable.Routes` vs `GlobalConfiguration.Configuration.Routes`), suas próprias classes base de controller (`System.Web.Mvc.Controller` vs `System.Web.Http.ApiController`) e suas próprias interfaces de restrição de rota (`IRouteConstraint` vs `IHttpRouteConstraint`). A ordem de chamada no `Global.asax.cs` era crítica: se as rotas do MVC fossem registradas antes da Web API, a rota padrão `{controller}/{action}/{id}` podia capturar requisições `/api/...` por engano, gerando erros difíceis de diagnosticar.
- No **.NET 10 (e no ecossistema moderno desde o .NET Core 3.0)**, existe um único mecanismo chamado **Endpoint Routing**. Todas as tecnologias (Minimal APIs, Controllers MVC, Razor Pages, SignalR e gRPC) compartilham a mesma tabela de rotas. O roteamento é integrado ao pipeline de middlewares, permitindo que políticas de segurança e autorização saibam qual endpoint atenderá a requisição antes mesmo de sua execução. Além disso, recursos como **Route Groups (`app.MapGroup()`)**, restrições tipadas inline (`{id:int}`) e o serviço `LinkGenerator` tornam o desenho de APIs mais limpo e seguro.

---

## Por que mudou?

1. **Unificação da Pilha Web:** No ecossistema antigo, desenvolvedores precisavam aprender duas APIs de roteamento diferentes para o mesmo projeto. No .NET moderno, controllers herdam de `ControllerBase` e usam o mesmo sistema de atributos e convenções.
2. **Fim dos Conflitos de Precedência:** A coexistência de tabelas separadas no `Global.asax.cs` era uma das maiores fontes de bugs em projetos legados. Com o Endpoint Routing do .NET 10, a resolução é feita em uma árvore única e otimizada de URLs.
3. **Restrições de Rota Declarativas (Inline Route Constraints):** No legado, aplicar regras como "o id deve ser inteiro" exigia instanciar dicionários de restrições ou classes auxiliares. No .NET 10, basta declarar `{id:int}` ou `{categoria:alpha}` diretamente na string da rota.
4. **Organização com Grupos de Rotas (`MapGroup`):** O .NET moderno permite agrupar endpoints sob um prefixo comum (`/api/v1/produtos`), aplicando filtros, prefixos de rota e metadados a todos os membros do grupo de uma só vez.
5. **Geração Tipada de URLs com `LinkGenerator`:** No legado, gerar links dependia de helpers acoplados ao contexto da requisição (`Url.Action`). No .NET 10, o serviço singleton `LinkGenerator` pode ser injetado em qualquer lugar (serviços de domínio, background workers) para gerar URLs canônicas com segurança.

---

## Comparativo Direto

| Dimensão Técnica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Mecanismo de Roteamento** | Duas tabelas isoladas: `RouteTable.Routes` (MVC) e `HttpRouteCollection` (API) | **Endpoint Routing** unificado para toda a aplicação |
| **Classes Base de Controller** | `System.Web.Mvc.Controller` separado de `System.Web.Http.ApiController` | Unificado em `ControllerBase` (API) e `Controller` (Views) |
| **Ponto de Registro** | Arquivos separados: `RouteConfig.cs` e `WebApiConfig.cs` | Centralizado no `Program.cs` |
| **Risco de Conflito de Rotas** | Alto: a ordem de registro no `Global.asax.cs` podia mascarar rotas da API | Inexistente: resolução em árvore determinística única |
| **Restrições de Rota (Constraints)** | Verbosas, exigindo classes ou dicionários com regex | Diretas e declarativas na URL: `{id:int}`, `{categoria:alpha}` |
| **Agrupamento de Rotas** | Não suportado de forma nativa (exigia bibliotecas ou repetição de strings) | Nativo via `app.MapGroup("/prefixo")` |
| **Minimal APIs** | Inexistente (obrigatório criar classes de Controller) | Suporte nativo completo com `app.MapGet()`, `app.MapPost()` |
| **Geração de Links** | Dependente do contexto HTTP (`Url.Action()`) | Desacoplado via injeção do serviço `LinkGenerator` |

---

## Diagrama Comparativo da Arquitetura de Rotas

```
MODELO LEGADO (.NET Framework 4.8.1): DUAS TABELAS SEPARADAS
┌──────────────────────────────────────────────────────────┐
│                   Global.asax.cs                         │
│  1. WebApiConfig.Register()   ──► Tabela de Rotas Web API│──► ApiController (JSON)
│  2. RouteConfig.Register()    ──► Tabela de Rotas MVC    │──► Controller (Views)
└──────────────────────────────────────────────────────────┘
(A ordem incorreta fazia a tabela MVC engolir chamadas da Web API)


MODELO MODERNO (.NET 10): ENDPOINT ROUTING UNIFICADO
┌──────────────────────────────────────────────────────────┐
│                   Program.cs                             │
│                  Endpoint Routing                        │
│                         │                                │
│       ┌─────────────────┼─────────────────┐              │
│       ▼                 ▼                 ▼              │
│ Minimal APIs      Controllers       Route Groups         │
│ app.MapGet()     [ApiController]   app.MapGroup()        │
└──────────────────────────────────────────────────────────┘
```

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── RoutingDemo.csproj                  # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Pacotes NuGet: MVC 5 e Web API 2
├── Web.config                          # Configurações do runtime e IIS
├── Global.asax                         # Ponto de entrada do HttpApplication
├── Global.asax.cs                      # Ordem de registro de WebApiConfig e RouteConfig
├── App_Start/
│   ├── RouteConfig.cs                  # Tabela de rotas do ASP.NET MVC 5
│   └── WebApiConfig.cs                 # Tabela separada de rotas do Web API 2
├── Models/
│   └── Produto.cs                      # Modelo de dados de produto
├── Controllers/
│   ├── ProdutosController.cs           # Controller herdando de System.Web.Mvc.Controller
│   └── Api/
│       └── ProdutosApiController.cs    # Controller herdando de System.Web.Http.ApiController
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config
│   ├── Shared/_Layout.cshtml
│   └── Produtos/
│       ├── Index.cshtml                # Lista de produtos via MVC
│       └── Detalhes.cshtml             # Detalhe de produto via MVC
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── RoutingDemo.csproj                  # SDK-style minimalista visando net10.0
├── Program.cs                          # Endpoint Routing, MapGroup, constraints e LinkGenerator
├── Models/
│   └── Produto.cs                      # Modelo de dados de produto
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5500
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)
Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/05-routing/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5500`).

Endpoints didáticos disponíveis:
- `GET /`: Painel visual interativo demonstrando resolução de rotas, geração de links com `LinkGenerator` e testes de constraints.
- `GET /api/produtos`: Retorna a lista completa de produtos via grupo de rotas `MapGroup`.
- `GET /api/produtos/1`: Retorna o produto com ID 1 (restrição numérica `{id:int}`).
- `GET /api/produtos/abc`: Retorna HTTP 404 automaticamente porque "abc" não satisfaz a restrição `{id:int}`.
- `GET /api/produtos/categoria/hardware`: Demonstra a rota com restrição alfabética `{categoria:alpha}`.

---

### Versão Legada (.NET Framework 4.8.1)
1. Abra o projeto `RoutingDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse:
   - `http://localhost:5501/`: View MVC de produtos renderizada pelo `ProdutosController`.
   - `http://localhost:5501/produtos/detalhes/1`: Detalhe do produto via MVC.
   - `http://localhost:5501/api/produtos`: Endpoint JSON do Web API processado pelo `ProdutosApiController`.
   - `http://localhost:5501/api/produtos/1`: Detalhe em JSON via Web API com restrição `{id:int}`.

Para compilar apenas via terminal (requer MSBuild do Windows):
```cmd
msbuild concepts\05-routing\framework\RoutingDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A separação física dos Controllers no Legado:** Veja que `ProdutosController.cs` herda de `System.Web.Mvc.Controller` (retorna Views), enquanto `ProdutosApiController.cs` herda de `System.Web.Http.ApiController` (retorna JSON). No .NET 10, essa divisão foi eliminada.
2. **O risco de ordem no `Global.asax.cs`:** No Framework, abra o `Global.asax.cs` e note que `WebApiConfig.Register` deve ser chamado antes de `RouteConfig.RegisterRoutes`. Se a ordem for invertida, a rota genérica do MVC tenta atender chamadas da API e falha.
3. **Grupos de Rotas no .NET 10:** Observe no `Program.cs` moderno a concisão de `app.MapGroup("/api/produtos")`, onde todos os endpoints filhos herdam o prefixo sem repetição de texto.
