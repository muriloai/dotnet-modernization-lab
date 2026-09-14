# Conceito 26: Princípios SOLID e Arquitetura Limpa

Este laboratório inicia o módulo de **Padrões de Projeto e Arquitetura**, demonstrando a transição de aplicações monolíticas com alto acoplamento no **.NET Framework 4.8.1** para uma estrutura baseada em **Arquitetura Limpa (Clean Architecture)** e aderência aos princípios **SOLID** no **.NET 10**.

---

## 1. Contexto e Motivação

No desenvolvimento tradicional com ASP.NET MVC e .NET Framework, era comum a formação de controladores sobrecarregados (conhecidos como "God Controllers"). Esses controladores acumulavam responsabilidades de validação de HTTP, regras de domínio, persistência direta via Entity Framework e integrações externas (e-mails, mensageria).

### No .NET Framework 4.8.1 (Legado):
- **Violação de SRP (Single Responsibility Principle):** Controllers realizavam o parsing da requisição, validavam regras de negócio, instanciavam conexões de banco (`new MyDbContext()`) e disparavam e-mails síncronos. Qualquer alteração em infraestrutura exigia reescrever os controllers.
- **Violação de DIP (Dependency Inversion Principle):** Módulos de alto nível dependiam diretamente de implementações concretas de baixo nível (`SqlConnection`, `SmtpClient`, `DbContext`).
- **Uso de Service Locator:** Para tentar contornar a falta de injeção de dependências nativa, equipes utilizavam o anti-padrão Service Locator via `DependencyResolver.Current.GetService<T>()`. Esse mecanismo ocultava as reais dependências da classe e transformava erros de configuração em falhas em tempo de execução.
- **Dificuldade de Testes Unitários:** O acoplamento estático tornava impossível testar regras de negócio sem conectar a um banco de dados real ou simular contextos complexos do IIS.

### No .NET 10 (Moderno):
- **Clean Architecture com Camadas Bem Definidas:**
  - **Domain:** Núcleo da aplicação contendo entidades e regras invariantes de negócio. Totalmente agnóstico de frameworks, bibliotecas ou bancos de dados.
  - **Application:** Casos de uso (Use Cases), DTOs e abstrações (`IClienteRepository`, `INotificadorService`). Define o que a aplicação faz.
  - **Infrastructure:** Implementações concretas de repositórios, mensageria e adaptadores de banco de dados.
  - **Presentation (API):** Controllers leves que apenas despacham comandos para os casos de uso injetados via DI nativa.
- **Aderência aos 5 Princípios SOLID:**
  - **S (Responsabilidade Única):** Cada classe de Use Case possui um único objetivo de negócio.
  - **O (Aberto/Fechado):** Novas regras de notificação ou desconto são adicionadas criando novas classes que implementam interfaces existentes, sem editar código já homologado.
  - **L (Substituição de Liskov):** Repositórios em memória para testes substituem perfeitamente repositórios de banco de dados sem alterar o comportamento do domínio.
  - **I (Segregação de Interfaces):** Interfaces granulares (`ILeituraRepository`, `IEscritaRepository`, `INotificadorEmail`) em vez de contratos gigantescos.
  - **D (Inversão de Dependências):** Todo o fluxo de controle depende de abstrações registradas nativamente no `IServiceCollection`.

---

## 2. Comparativo Técnico Estruturado

| Princípio / Prática | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Responsabilidade dos Controllers** | God Controller: validação, negócio, SQL e notificação | Thin Controller: apenas delega a execução para o Use Case |
| **Acesso a Dados** | `new ApplicationDbContext()` instanciado no controller | Interfaces de repositório injetadas via construtor |
| **Inversão de Controle** | Service Locator ou frameworks de terceiros manuais | Contêiner nativo de DI do .NET com escopos definidos |
| **Isolamento do Domínio** | Regras misturadas a classes do ASP.NET e System.Web | Entidades puras sem dependências externas |
| **Testabilidade** | Exige mocks complexos de HttpContext e banco real | Testes unitários puros instanciando o Use Case com fakes |

---

## 3. Estrutura dos Projetos

```text
concepts/26-solid-architecture/
├── README.md
├── framework/                              # Projeto Legado (.NET Framework 4.8.1)
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── ClientesLegadoController.cs
│   ├── Global.asax
│   │   └── Global.asax.cs
│   ├── Models/
│   │   └── ClienteAcoplado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Views/
│   │   ├── ClientesLegado/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── SolidArchitectureDemo.csproj
│   ├── Web.config
│   └── packages.config
└── net10/                                  # Projeto Moderno (.NET 10 - Clean Architecture)
    ├── Application/
    │   ├── DTOs/
    │   │   ├── CadastrarClienteCommand.cs
    │   │   └── ClienteResponse.cs
    │   ├── Interfaces/
    │   │   ├── IClienteRepository.cs
    │   │   └── INotificadorService.cs
    │   └── UseCases/
    │       └── CadastrarClienteUseCase.cs
    ├── Controllers/
    │   └── ClientesController.cs
    ├── Domain/
    │   └── Entities/
    │       └── Cliente.cs
    ├── Infrastructure/
    │   ├── Repositories/
    │   │   └── ClienteRepositoryEmMemoria.cs
    │   └── Services/
    │       └── NotificadorConsoleService.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── Program.cs
    ├── SolidArchitectureDemo.csproj
    └── wwwroot/
        ├── css/
        │   └── site.css
        └── index.html
```

---

## 4. Portas dos Projetos

| Projeto | Runtime | Porta HTTP | Servidor |
| :--- | :--- | :--- | :--- |
| `net10/` | .NET 10 | `http://localhost:7600` | Kestrel |
| `framework/` | .NET Framework 4.8.1 | `http://localhost:7601` | IIS Express |

---

## 5. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)
```bash
cd concepts/26-solid-architecture/net10
dotnet run
```
Acesse `http://localhost:7600` para cadastrar clientes através do Use Case desacoplado, inspecionar as camadas da Clean Architecture e verificar a injeção de dependências.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra `SolidArchitectureDemo.csproj` no Visual Studio 2022.
2. Inicie com `Ctrl + F5` no IIS Express na porta `7601`.
3. Veja o controller monolítico herdado gerenciando validação, persistência e resposta diretamente.
