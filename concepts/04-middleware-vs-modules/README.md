# Lab 04: Middlewares vs Módulos HTTP

> Comparativo prático entre o modelo de processamento de requisições baseado em eventos do IIS (`IHttpModule` e `IHttpHandler` em XML) do **.NET Framework 4.8.1** e o pipeline linear, assíncrono e bidirecional de **Middlewares** (`app.Use()`, `app.Map()` e `RequestDelegate`) do **.NET 10**.

---

## O que é este conceito?

O **pipeline de requisição HTTP** determina como uma mensagem recebida pela rede é interceptada, enriquecida, validada e respondida pela aplicação web.

- No **.NET Framework (1.0 até 4.8.1)**, o processamento de requisições era amarrado ao ciclo de vida de eventos do Windows IIS (`HttpApplication`). Para interceptar requisições, criavam-se **Módulos HTTP** (`IHttpModule`) conectados a eventos fixos (como `BeginRequest`, `AuthenticateRequest`, `EndRequest`). Para criar pontos de extremidade de resposta direta sem passar pelo MVC, usavam-se **Handlers HTTP** (`IHttpHandler` ou arquivos `.ashx`). Módulos e Handlers precisavam ser registrados manualmente no XML do `Web.config`, e o acesso aos dados da requisição dependia da propriedade estática global `HttpContext.Current`.
- No **.NET 10 (e no ecossistema moderno desde o .NET Core)**, o processamento ocorre através de uma cadeia linear e bidirecional de **Middlewares**. Cada middleware é uma função ou classe que recebe o `HttpContext` e um delegado `RequestDelegate next`. O middleware executa código antes de repassar a execução para o próximo elo da cadeia e executa código adicional quando a resposta retorna. Não há arquivos XML nem eventos fixos de servidor; tudo é configurado diretamente em C# no `Program.cs`.

---

## Por que mudou?

1. **Ordem de Execução Clara e Determinística:** No .NET Framework, a ordem de execução era governada pelas etapas fixas do IIS (mais de 20 eventos em ordem pré-determinada). No .NET 10, a ordem é exatamente a sequência linear em que os métodos `app.Use...()` são chamados no código C#.
2. **Fluxo Bidirecional Natural (Envolvimento com `await next()`):** Para medir o tempo de uma requisição no legado, era necessário capturar a hora de início no evento `BeginRequest`, guardar o valor em um dicionário estático (`HttpContext.Items`) e calcular a diferença no evento `EndRequest`. No .NET 10, o middleware envolve a chamada downstream naturalmente com um `Stopwatch` ou bloco `try/catch`.
3. **Fim do XML para Componentes de Rede:** No legado, esquecer de registrar uma tag `<add name="..." type="..." />` em `<system.webServer><modules>` impedia que o módulo funcionasse. No .NET 10, o pipeline é fortemente tipado e configurado no `Program.cs`.
4. **Desacoplamento do Windows IIS:** Os eventos do `HttpApplication` foram desenhados para o ecossistema Windows/IIS. O pipeline de middlewares do ASP.NET Core roda sobre qualquer servidor (Kestrel, IIS, contêiner Linux, macOS).
5. **Substituição de Handlers (.ashx) por Minimal APIs e Branches:** No legado, handlers eram classes isoladas que implementavam `ProcessRequest` para endpoints leves (ex.: `/health`). No .NET moderno, isso é feito de forma simples com `app.Map("/health", ...)` ou Minimal APIs.

---

## Comparativo Direto

| Dimensão Técnica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Mecanismo de Interceptação** | `IHttpModule` acoplado a eventos do IIS | Middlewares de classe ou delegates inline (`app.Use`) |
| **Respondedores Terminais** | `IHttpHandler` (frequentemente arquivos `.ashx`) | Endpoints terminais ou Minimal APIs (`app.MapGet`, `app.Run`) |
| **Registro dos Componentes** | XML no `Web.config` em `<system.webServer>` | Código C# explícito no `Program.cs` |
| **Ordem de Execução** | Determinada pelas fases fixas do ciclo de vida do IIS | Determinada rigorosamente pela sequência em `Program.cs` |
| **Acesso ao Contexto** | `HttpContext.Current` estático | `HttpContext` injetado como argumento |
| **Medição e Captura de Erros** | Eventos separados (`BeginRequest` e `EndRequest`) | Envolvimento direto com `await next(context)` |
| **Ramificação de Pipeline** | Inexistente (módulos rodam para todas as requisições) | Nativo via `app.Map()` e `app.UseWhen()` |
| **Testabilidade** | Difícil devido ao estado estático global | Imediata usando mocks ou instâncias de `DefaultHttpContext` |

---

## Diagrama do Fluxo Bidirecional de Middlewares (.NET 10)

```
Requisição HTTP Recebida
         │
         ▼
┌─────────────────────────────────┐
│     Middleware 1: Timing        │
│  - Inicia Stopwatch             │
│  - Chama await next(context) ─────────┐
│  - Registra tempo total         │     │
│  - Adiciona header de resposta  │     │
└─────────────────────────────────┘     │
         ▲                              ▼
         │             ┌─────────────────────────────────┐
         │             │  Middleware 2: Headers Custom   │
         │             │  - Executa lógica prévia        │
         │             │  - Chama await next(context) ─────────┐
         │             │  - Executa pós-processamento    │     │
         │             └─────────────────────────────────┘     │
         │                              ▲                      ▼
         │                              │             ┌─────────────────┐
         │                              │             │ Endpoint Final  │
         │                              │             │ (Ex: Controller │
         │                              │             │ ou Minimal API) │
         │                              │             └─────────────────┘
         │                              │                      │
         │                              └──────────────────────┘
         │                                   Resposta Gerada
         └─────────────────────────────────────────────────────┘
                                  │
                                  ▼
                        Resposta HTTP Enviada
```

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── MiddlewareVsModulesDemo.csproj       # Projeto no formato clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Pacotes NuGet do ASP.NET MVC 5
├── Web.config                          # Registro de módulos e handlers em <system.webServer>
├── Global.asax                         # Arquivo do ciclo de vida da aplicação
├── Global.asax.cs                      # Eventos de aplicação: Application_Start
├── App_Start/
│   └── RouteConfig.cs                  # Roteamento MVC clássico
├── Modules/
│   ├── RequestTimingModule.cs          # IHttpModule medindo tempo via BeginRequest/EndRequest
│   └── CustomHeaderModule.cs           # IHttpModule adicionando headers de resposta
├── Handlers/
│   ├── HealthCheckHandler.ashx         # Ponto de extremidade terminal legado
│   └── HealthCheckHandler.ashx.cs      # IHttpHandler respondendo status em texto
├── Controllers/
│   └── HomeController.cs               # Controller para renderização da página principal
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config
│   ├── Shared/_Layout.cshtml
│   └── Home/
│       └── Index.cshtml                # Visualização didática dos cabeçalhos recebidos
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── MiddlewareVsModulesDemo.csproj      # SDK-style minimalista visando net10.0
├── Program.cs                          # Pipeline explícito com app.Use, app.Map e app.UseWhen
├── Middleware/
│   ├── RequestTimingMiddleware.cs      # Middleware que mede tempo com Stopwatch
│   └── CustomHeaderMiddleware.cs       # Middleware que injeta headers de identificação
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5400
└── wwwroot/
    └── css/site.css                    # Estilos visuais compartilhados
```

---

## Como Executar

### Versão Moderna (.NET 10)
Navegue até a pasta `net10/` e execute via CLI do .NET:

```bash
cd concepts/04-middleware-vs-modules/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5400`).

Endpoints didáticos disponíveis:
- `GET /`: Painel visual didático que executa o pipeline completo e exibe os cabeçalhos de resposta gerados pelos middlewares.
- `GET /health`: Endpoint terminal ramificado via `app.Map("/health")` que responde diretamente sem passar pelo restante da aplicação (equivalente moderno ao `.ashx`).
- `GET /api/ping`: Endpoint que aciona o pipeline condicional configurado com `app.UseWhen()`.

---

### Versão Legada (.NET Framework 4.8.1)
1. Abra o projeto `MiddlewareVsModulesDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse:
   - `http://localhost:5401/`: View principal processada pelos módulos HTTP.
   - `http://localhost:5401/Handlers/HealthCheckHandler.ashx`: Handler HTTP direto.

Para compilar apenas via terminal (requer MSBuild do Windows):
```cmd
msbuild concepts\04-middleware-vs-modules\framework\MiddlewareVsModulesDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A inspeção dos Headers no DevTools do Navegador:**
   - Abra a aba Rede (Network) das ferramentas de desenvolvedor do seu navegador (F12).
   - Recarregue a página e observe os cabeçalhos de resposta: `X-Response-Time-Ms` e `X-Execution-Engine`.
   - Veja como no .NET 10 esses cabeçalhos foram adicionados pelo fluxo de retorno do middleware através de `context.Response.OnStarting()`.
2. **Ramificação com `app.Map`:** Veja no `Program.cs` do .NET 10 como a rota `/health` é isolada em um mini-pipeline próprio, sem carregar middlewares desnecessários.
3. **Simplicidade do Código C#:** Compare a complexidade de criar e registrar um módulo HTTP em XML no `Web.config` do Framework com a declaração direta de poucas linhas de middleware no .NET 10.
