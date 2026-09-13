# Lab 03: Injeção de Dependências

> Comparativo prático entre o modelo do **.NET Framework 4.8.1** (ausência de container nativo, uso de bibliotecas de terceiros como Unity, separação entre resolvers de MVC e Web API, e o anti-pattern Service Locator) e o modelo moderno do **.NET 10** (container nativo padronizado, ciclos de vida Transient/Scoped/Singleton, Keyed Services e construtores primários do C#).

---

## O que é este conceito?

A **Injeção de Dependências (Dependency Injection - DI)** é uma técnica de Inversão de Controle (IoC) que permite que classes recebam suas dependências de fontes externas em vez de criá-las diretamente através de `new`.

- No **.NET Framework (1.0 até 4.8.1)**, não havia container de injeção de dependências embutido no runtime. O framework instanciava controllers através de reflection padrão com construtores vazios. Para utilizar injeção, os times precisavam adotar bibliotecas externas (como Unity, Autofac, Ninject ou Castle Windsor) e implementar interfaces específicas de resolução (`IDependencyResolver`). Era comum também a proliferação do anti-pattern **Service Locator** (`DependencyResolver.Current.GetService<T>()`), que esconde dependências e mascara falhas.
- No **.NET 10 (e no ecossistema moderno desde o .NET Core)**, a injeção de dependências é nativa, integrada no núcleo da plataforma através do pacote `Microsoft.Extensions.DependencyInjection`. Toda a arquitetura do ASP.NET Core (middlewares, controllers, minimal APIs, filtros e background services) é alimentada pelo mesmo container `IServiceProvider`, com suporte nativo a ciclos de vida estritos e **Keyed Services**.

---

## Por que mudou?

1. **Fim da Fragmentação de Containers:** No legado, cada biblioteca de injeção possuía sua própria sintaxe de configuração, gerenciamento de ciclos de vida e extensões. A padronização em torno de `IServiceCollection` unificou todo o ecossistema .NET.
2. **Eliminação dos Resolvers Duplicados:** No .NET Framework, o ASP.NET MVC usava `System.Web.Mvc.IDependencyResolver`, enquanto o ASP.NET Web API usava `System.Web.Http.Dependencies.IDependencyResolver`. Em projetos que combinavam páginas e APIs, era necessário configurar dois resolvers diferentes para o mesmo container.
3. **Erradicação do Anti-Pattern Service Locator:** No legado, a chamada `DependencyResolver.Current.GetService<T>()` permitia buscar qualquer serviço em qualquer ponto do código, quebrando o encapsulamento e impedindo que o compilador avisasse sobre dependências ausentes. A injeção por construtor tornou-se a regra padrão.
4. **Ciclos de Vida Transparentes e Seguros:** No legado, gerenciar o escopo de uma requisição HTTP exigia módulos manuais ou extensões específicas do container externo. No .NET moderno, os ciclos `Transient`, `Scoped` e `Singleton` são nativos e gerenciados pelo servidor Kestrel por requisição HTTP.
5. **Keyed Services Nativos:** Desde o .NET 8 e aprimorado no .NET 10, o container nativo permite registrar múltiplas implementações de uma mesma interface associadas a chaves nominais, sem a necessidade de fábricas complexas.

---

## Comparativo Direto

| Dimensão Técnica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Container Nativo** | Inexistente (requer Unity, Autofac, Ninject ou instanciação manual) | Integrado nativamente (`IServiceCollection`, `IServiceProvider`) |
| **Ponto de Registro** | Arquivos de inicialização customizados (`UnityConfig.cs`) chamados no `Global.asax.cs` | `builder.Services` centralizado no `Program.cs` |
| **Integração MVC e Web API** | Interfaces duplicadas (`System.Web.Mvc.IDependencyResolver` vs `System.Web.Http.Dependencies.IDependencyResolver`) | Pilha web unificada usando o mesmo provedor de serviços |
| **Ciclos de Vida** | Dependentes da implementação do container externo | Padronizados: `Transient`, `Scoped`, `Singleton` |
| **Keyed Services** | Exigia factories manuais ou dicionários customizados | Suporte nativo via `AddKeyedScoped`, `AddKeyedSingleton` e `[FromKeyedServices]` |
| **Sintaxe de Construtor** | Construtores tradicionais com atribuição explícita de campos | Suporte a **Primary Constructors** do C# moderno (`public class MeuServico(IDep dep)`) |
| **Prática Comum Inadequada** | Uso frequente de `DependencyResolver.Current.GetService<T>()` | Injeção explícita por construtor ou parâmetros de endpoint |

---

## Ciclos de Vida (Lifetimes) no .NET 10

```
┌────────────────────────────────────────────────────────────────────────┐
│                              SINGLETON                                 │
│  Uma única instância compartilhada durante toda a vida da aplicação    │
└────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────┐  ┌──────────────────────────────────┐
│        REQUISIÇÃO HTTP 1         │  │        REQUISIÇÃO HTTP 2         │
│                                  │  │                                  │
│  ┌────────────────────────────┐  │  │  ┌────────────────────────────┐  │
│  │           SCOPED           │  │  │  │           SCOPED           │  │
│  │ Mesma instância no escopo  │  │  │  │ Nova instância para este   │  │
│  │ desta requisição           │  │  │  │ escopo específico          │  │
│  └────────────────────────────┘  │  │  └────────────────────────────┘  │
│                                  │  │                                  │
│  ┌──────────────┐┌─────────────┐ │  │  ┌──────────────┐┌─────────────┐ │
│  │  TRANSIENT   ││  TRANSIENT  │ │  │  │  TRANSIENT   ││  TRANSIENT  │ │
│  │  Instância A ││ Instância B │ │  │  │  Instância C ││ Instância D │ │
│  └──────────────┘└─────────────┘ │  │  └──────────────┘└─────────────┘ │
└──────────────────────────────────┘  └──────────────────────────────────┘
```

1. **Transient (`AddTransient`):**
   - Cria uma nova instância a cada vez que o serviço é solicitado.
   - Ideal para componentes leves e sem estado (*stateless*).

2. **Scoped (`AddScoped`):**
   - Cria uma instância por escopo (no contexto web, uma instância por requisição HTTP).
   - Todos os componentes que solicitarem o serviço durante a mesma requisição recebem a mesma referência.
   - Essencial para contextos de banco de dados e transações.

3. **Singleton (`AddSingleton`):**
   - Cria uma única instância na primeira solicitação e a reutiliza em todas as requisições subsequentes.
   - Deve ser utilizado com atenção para garantir segurança em acessos concorrentes (*thread safety*).

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── DependencyInjectionDemo.csproj       # Projeto no formato clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Pacotes NuGet do ASP.NET MVC 5
├── Web.config                          # Configurações XML padrão
├── Global.asax                         # Arquivo do ciclo de vida da aplicação
├── Global.asax.cs                      # Registro do resolver em Application_Start
├── App_Start/
│   ├── RouteConfig.cs                  # Roteamento MVC clássico
│   └── CustomDependencyResolver.cs     # Implementação manual de IDependencyResolver
├── Services/
│   ├── IClienteService.cs              # Contrato de serviço de clientes
│   ├── ClienteService.cs               # Implementação do serviço
│   ├── INotificacaoService.cs          # Contrato de serviço de notificações
│   ├── EmailNotificacaoService.cs      # Implementação de notificação
│   └── ILifetimeDemoService.cs         # Serviços para demonstrar identidades de instâncias
├── Controllers/
│   └── DiDemoController.cs             # Injeção por construtor e exemplo de Service Locator
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config
│   ├── Shared/_Layout.cshtml
│   └── DiDemo/
│       └── Index.cshtml                # Visualização das instâncias e identificadores
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── DependencyInjectionDemo.csproj      # SDK-style minimalista visando net10.0
├── Program.cs                          # Registro dos serviços, Keyed Services e endpoints
├── Services/
│   ├── IClienteService.cs              # Contrato do serviço
│   ├── ClienteService.cs               # Implementação usando Primary Constructor
│   ├── INotificacaoService.cs          # Contrato de notificação
│   ├── EmailNotificacaoService.cs      # Implementação para e-mail (chave "email")
│   ├── SmsNotificacaoService.cs        # Implementação para SMS (chave "sms")
│   └── ILifetimeServices.cs            # ITransientService, IScopedService, ISingletonService
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5300
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)
Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/03-dependency-injection/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5300`).

Endpoints didáticos disponíveis:
- `GET /`: Painel visual interativo exibindo a resolução de instâncias e os GUIDs dos ciclos de vida.
- `GET /api/di/lifetimes`: Retorna duas resoluções de cada ciclo de vida na mesma requisição, permitindo verificar que o Transient muda a cada resolução, o Scoped permanece idêntico dentro da requisição e o Singleton nunca muda.
- `GET /api/di/keyed`: Demonstra a injeção de implementações diferentes para a mesma interface usando Keyed Services (`[FromKeyedServices("email")]` e `[FromKeyedServices("sms")]`).
- `GET /api/di/clientes`: Demonstra a execução de serviço de negócio injetado via Primary Constructor.

---

### Versão Legada (.NET Framework 4.8.1)
1. Abra o projeto `DependencyInjectionDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse `http://localhost:5301/` para inspecionar os serviços resolvidos pelo `CustomDependencyResolver`.

Para compilar apenas via terminal (requer MSBuild do Windows):
```cmd
msbuild concepts\03-dependency-injection\framework\DependencyInjectionDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A diferença entre os GUIDs em `/api/di/lifetimes`:**
   - Observe que as duas resoluções de `ITransientService` possuem GUIDs diferentes na mesma requisição.
   - Observe que as duas resoluções de `IScopedService` possuem exatamente o mesmo GUID na mesma requisição, mas mudam se você atualizar a página.
   - Observe que `ISingletonService` preserva o mesmo GUID em qualquer requisição, enquanto a aplicação estiver ativa.
2. **Keyed Services:** Veja como o .NET 10 resolve diferentes implementações de `INotificacaoService` sem a necessidade de criar factories intermediárias.
3. **Anti-pattern Service Locator no Legado:** Veja no controller do Framework a diferença entre a injeção via construtor e a chamada imperativa `DependencyResolver.Current.GetService<T>()`.
