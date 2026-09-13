# Lab 01: Estrutura de Projeto e Bootstrap (Inicialização)

> Comparativo prático entre a inicialização clássica baseada em **IIS + `Global.asax.cs`** do **.NET Framework 4.8.1** e o modelo moderno **Kestrel + `Program.cs` com Top-Level Statements** do **.NET 10**.

---

## O que é este conceito?

A **estrutura de projeto e o modelo de inicialização (bootstrap)** definem como uma aplicação .NET é declarada nos arquivos do sistema, como suas dependências são resolvidas, como o processo hospedeiro é carregado e como as rotas e componentes iniciais são registrados.

- No **.NET Framework (1.0 até 4.8.1)**, uma aplicação web era um módulo hospedado dentro do processo de trabalho do Windows IIS (`w3wp.exe`), orientado por eventos fixos do `HttpApplication`.
- No **.NET 10 (e no ecossistema .NET moderno desde o .NET Core)**, a aplicação é um executável de console autocontido e autohospedado (_self-hosted_) rodando sobre o servidor multiplataforma de altíssima performance **Kestrel**.

---

## Por que mudou?

1. **Eliminação do Acoplamento ao IIS e Windows:** A biblioteca `System.Web.dll` do .NET Framework possuía chamadas diretas à API Win32 e ao registro do Windows, impossibilitando a execução em contêineres Linux ou macOS.
2. **Fim dos Conflitos de Merge no `.csproj`:** No formato antigo, cada arquivo `.cs` adicionado ao projeto exigia uma tag `<Compile Include="Arquivo.cs" />`. Em equipes com múltiplos desenvolvedores, conflitos de mesclagem no XML eram diários.
3. **Redução Drástica de Boilerplate:** O C# moderno introduziu _Top-Level Statements_ (C# 9+) e _Implicit Usings_ (C# 10+), permitindo configurar uma API completa em poucas linhas, sem a cerimônia de namespaces aninhados e métodos `static void Main`.
4. **Gerenciamento de Pacotes Sem `bindingRedirect`:** O `packages.config` exigia que versões de bibliotecas fossem amarradas manualmente no `Web.config`. O `PackageReference` do .NET 10 resolve dependências transitivas no grafo do NuGet de forma transparente.

---

## Comparativo Direto

| Dimensão Técnica           | .NET Framework 4.8.1 (Legado)                                                           | .NET 10 (Moderno)                                                                     |
| -------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Arquivo de Projeto**     | `ProjectStructure.csproj` (XML com 100+ linhas, GUIDs de tipo e referências explícitas) | `ProjectStructure.csproj` (SDK-style com ~8 linhas e inclusão automática de arquivos) |
| **Ponto de Entrada**       | `Global.asax.cs` -> Evento `Application_Start`                                          | `Program.cs` -> _Top-Level Statements_ diretos                                        |
| **Servidor Web**           | Windows IIS / IIS Express obrigatório (`w3wp.exe`)                                      | Kestrel embutido, autohospedado e multiplataforma                                     |
| **Plataformas Suportadas** | Exclusivo Windows                                                                       | Linux (Ubuntu, Alpine, Chiseled), macOS, Windows                                      |
| **Configuração**           | `Web.config` monolítico em XML                                                          | `appsettings.json` hierárquico + `IConfiguration`                                     |
| **Gerenciamento NuGet**    | `packages.config` com `<bindingRedirect>` no Web.config                                 | `PackageReference` transitivo com cache global                                        |
| **Versão da Linguagem**    | Preso ao **C# 7.3** (sem novidades do runtime)                                          | Suporte nativo ao **C# 14**                                                           |
| **Pilhas Web**             | MVC 5 e Web API 2 separadas (`Controller` vs `ApiController`)                           | Unificado em `ControllerBase` e **Minimal APIs**                                      |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`

```
framework/
├── ProjectStructure.csproj     # Definição do projeto no formato MSBuild clássico
├── packages.config             # Lista de pacotes NuGet instalados
├── Web.config                  # Configurações do runtime, IIS e assembly redirects
├── Global.asax                 # Declaração do manipulador HttpApplication
├── Global.asax.cs              # Código C# do ciclo de vida: Application_Start
├── App_Start/
│   ├── RouteConfig.cs          # Roteamento do ASP.NET MVC 5
│   ├── WebApiConfig.cs         # Roteamento separado do Web API 2
│   └── FilterConfig.cs         # Registro de filtros globais
├── Controllers/
│   ├── HomeController.cs       # Controller herdando de System.Web.Mvc.Controller
│   └── Api/
│       └── InfoApiController.cs# Controller herdando de System.Web.Http.ApiController
├── Views/
│   ├── Web.config              # Impede acesso direto aos arquivos .cshtml
│   ├── _ViewStart.cshtml       # Define o layout mestre padrão
│   ├── Shared/_Layout.cshtml   # Template visual compartilhado
│   └── Home/Index.cshtml       # Página Razor renderizada
└── Content/Site.css            # Estilização básica
```

### Projeto Moderno: `net10/`

```
net10/
├── ProjectStructure.csproj     # SDK-style: <Project Sdk="Microsoft.NET.Sdk.Web">
├── Program.cs                  # Configuração de serviços, middlewares e endpoints
├── appsettings.json            # Configuração padrão em JSON
├── appsettings.Development.json# Configuração específica para ambiente de desenvolvimento
├── Properties/
│   └── launchSettings.json     # Perfis de inicialização da CLI e IDE
└── wwwroot/
    └── css/site.css            # Arquivos estáticos servidos pelo Kestrel
```

---

## Como Executar

### Versão Moderna (.NET 10)

Basta navegar até a pasta `net10/` e utilizar a CLI oficial do .NET:

```bash
cd concepts/01-project-structure/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5100`).

Endpoints didáticos disponíveis:

- `GET /`: Página HTML didática com dados do runtime.
- `GET /api/info`: JSON detalhando as propriedades do ambiente .NET 10.
- `GET /api/comparison`: Objeto JSON estruturado comparando as duas plataformas.

---

### Versão Legada (.NET Framework 4.8.1)

Por depender da infraestrutura do Windows IIS, utilize o Visual Studio 2022:

1. Abra a pasta ou a solução no **Visual Studio 2022** (com a carga de trabalho _ASP.NET and web development_).
2. Defina o projeto `ProjectStructure.csproj` em `concepts/01-project-structure/framework/` como projeto de inicialização.
3. Pressione **Ctrl + F5** para executar via **IIS Express**.
4. Acesse os endpoints:
   - `http://localhost:5101/`: View MVC 5.
   - `http://localhost:5101/api/info`: Endpoint Web API 2.

Para compilar apenas via terminal (requer MSBuild do Windows):

```cmd
msbuild concepts\01-project-structure\framework\ProjectStructure.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **Tamanho do Arquivo `.csproj`:** Compare as 10 linhas do `net10/ProjectStructure.csproj` contra as mais de 100 linhas do `framework/ProjectStructure.csproj`.
2. **Duplicidade de Controllers no Legado:** Veja como no Framework o `HomeController` usa `System.Web.Mvc` e o `InfoApiController` precisa de `System.Web.Http`. No .NET 10, essa divisão não existe mais.
3. **Desacoplamento do IIS:** No .NET 10, a chamada `app.Run()` inicia um servidor Kestrel autônomo dentro do próprio processo, sem depender de módulos nativos do Windows.
