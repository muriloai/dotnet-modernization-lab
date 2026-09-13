# Lab 09: Padrão Repositório e Unit of Work

> Comparativo prático entre o uso tradicional de repositórios genéricos e Unit of Work no **.NET Framework 4.8.1** (`IRepository<T>`, `IUnitOfWork` com `SaveChanges` manual, vazamento de `IQueryable` e sobrecarga de camadas sobre o EF 6) e a evolução moderna no **.NET 10** (uso direto do `DbContext` injetado como Unit of Work nativo com projeções diretas para DTOs vs repositórios específicos orientados a domínio com regras de negócio e transações assíncronas atômicas).

---

## O que é este conceito?

O **Padrão Repositório (Repository Pattern)** tem como objetivo mediar a comunicação entre a camada de lógica de negócio e a camada de mapeamento de dados, agindo como uma coleção em memória de objetos de domínio. O **Unit of Work** coordena múltiplos repositórios compartilhando uma mesma transação ou contexto de persistência.

- No **.NET Framework (com Entity Framework 6)**:
  - Era praxe construir interfaces genéricas como `IRepository<T>` e `IUnitOfWork` por cima do EF 6.
  - A principal justificativa era contornar a dificuldade de testar unitariamente o `DbContext` antigo e a promessa teórica de poder trocar de ORM sem alterar o resto da aplicação.
  - Na prática, gerava-se uma "abstração sobre outra abstração", visto que o `DbSet<T>` já atua como um repositório e o `DbContext` já gerencia transações como um Unit of Work.
  - Surgiam dois problemas críticos: ou o repositório expunha `IQueryable<T>` (vazando detalhes de consulta e gerando falhas por conexões fechadas antes da hora), ou forçava assinaturas engessadas com coleções de expressões lambda para tentar simular includes e ordenações (`Expression<Func<T, object>>[]`).
- No **.NET 10 (com EF Core 10)**:
  - O `DbContext` é projetado especificamente para injeção de dependências e testabilidade de alta fidelidade (com SQLite em memória ou provedor InMemory).
  - A comunidade moderna e o próprio time de engenharia da Microsoft adotam a abordagem pragmática: **injetar o `DbContext` diretamente nos serviços ou controladores**. Isso elimina centenas de linhas de código inútil e permite tirar proveito total de projeções com DTOs (`.Select()`), `AsNoTracking()` e métodos em lote.
  - Quando o padrão repositório ainda é utilizado, ele não é genérico: constroem-se **Repositórios de Domínio (DDD)** com interfaces semânticas focadas em agregados (por exemplo, `IPedidoRepository.ObterExtratoDoClienteAsync(int clienteId)`), encapsulando queries complexas e transações explícitas com `context.Database.BeginTransactionAsync()`.

---

## Por que mudou?

1. **Eliminação de Código Redundante (Boilerplate):** Criar `IRepository<T>` com métodos como `GetById`, `GetAll`, `Add` e `Remove` apenas para repassar chamadas para `_dbSet.Find()` ou `_dbSet.Add()` não trazia valor real e dobrava a quantidade de arquivos no projeto.
2. **Resolução do Dilema do `IQueryable`:** Repositórios genéricos que devolviam `IQueryable<T>` permitiam que filtros LINQ fossem montados nos controllers, quebrando o objetivo do padrão. Ao remover essa camada artificial, a aplicação utiliza LINQ de forma nativa onde a consulta precisa ser montada.
3. **Eficiência de Projeções Diretas para DTOs:** No modelo com repositórios genéricos clássicos, era comum buscar entidades inteiras com todos os seus relacionamentos para só depois converter para ViewModels. No .NET moderno, o LINQ projeta os dados diretamente para records ou DTOs no comando `SELECT`, transportando apenas as colunas necessárias da base.
4. **Testabilidade Real sem Mocks Frágeis:** No EF 6, mockar `DbSet` e queries assíncronas exigia dezenas de linhas de código de setup de teste. No EF Core, testes rápidos podem ser executados contra SQLite em memória (`Data Source=:memory:`) com comportamento idêntico ao banco real.
5. **Transações Modernas e Assíncronas:** No legado, o controle de transações dependia de `TransactionScope` síncrono ou do `DbContext.Database.BeginTransaction()`. No .NET 10, o controle de transações assíncronas atômicas com `await using var transaction = await context.Database.BeginTransactionAsync()` é limpo e seguro.

---

## Comparativo Direto

| Recurso / Abordagem | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Abstração Adotada** | Repositório Genérico `IRepository<T>` obrigatório | Uso direto de `DbContext` ou Repositórios de Domínio (DDD) |
| **Papel do Unit of Work** | Classe manual `UnitOfWork` que chama `context.SaveChanges()` | O próprio `DbContext` injetado como Scoped com `SaveChangesAsync()` |
| **Projeções de Dados** | Carrega entidades completas e mapeia em memória | Projeção direta via `.Select(p => new Dto(...))` gerando SQL enxuto |
| **Eager Loading (Includes)** | Passagem de parâmetros complexos `params Expression<Func<T, object>>[]` | Chamadas diretas de `.Include()` e `.AsSplitQuery()` no LINQ |
| **Transações Complexas** | `TransactionScope` clássico ou métodos síncronos | `await using var tx = await context.Database.BeginTransactionAsync()` |
| **Testabilidade** | Setup complexo de mocks de `DbSet` e `IDbAsyncQueryProvider` | Instanciação direta com SQLite em memória sem necessidade de mocks |
| **Volume de Código** | Alto: dezenas de interfaces, classes e wrappers redundantes | Baixo: código direto, expressivo e com menos pontos de falha |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── RepositoryPatternDemo.csproj        # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Dependências: EntityFramework 6.4.4 e MVC 5
├── Web.config                          # Conexão e providers do EF 6
├── Global.asax                         # Inicialização
├── Global.asax.cs                      # Registro de rotas e DbInitializer
├── App_Start/
│   └── RouteConfig.cs                  # Roteamento MVC
├── Models/
│   ├── Cliente.cs                      # Entidade Cliente
│   ├── Pedido.cs                       # Entidade Pedido
│   └── ItemPedido.cs                   # Entidade ItemPedido
├── Data/
│   ├── AppDbContext.cs                 # DbContext com DbModelBuilder
│   └── DbInitializer.cs                # Inicializador com carga de dados
├── Repositories/
│   ├── IRepository.cs                  # Interface de repositório genérico clássico
│   ├── GenericRepository.cs            # Implementação genérica sobre DbSet<T>
│   ├── IUnitOfWork.cs                  # Interface clássica de Unit of Work
│   └── UnitOfWork.cs                   # Implementação clássica de Unit of Work
├── Controllers/
│   └── PedidosController.cs            # Controller consumindo IUnitOfWork
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config
│   ├── Shared/_Layout.cshtml
│   └── Pedidos/
│       ├── Index.cshtml                # Listagem de pedidos via Unit of Work
│       └── Criar.cshtml                # Criação de pedido
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── RepositoryPatternDemo.csproj        # SDK-style visando net10.0 com EF Core Sqlite
├── Program.cs                          # Configuração de AddDbContext e injeção de dependências
├── appsettings.json                    # Connection string SQLite
├── Models/
│   ├── Cliente.cs                      # Entidade Cliente
│   ├── Pedido.cs                       # Entidade Pedido
│   ├── ItemPedido.cs                   # Entidade ItemPedido
│   └── DTOs/
│       └── PedidoResumoDto.cs          # Record DTO para consultas projetadas
├── Data/
│   ├── AppDbContext.cs                 # DbContext moderno com Fluent API
│   └── DbInitializer.cs                # Inicialização assíncrona da base
├── Repositories/
│   ├── IPedidoRepository.cs            # Repositório de domínio específico com intenção de negócio
│   └── PedidoRepository.cs            # Implementação com transações atômicas assíncronas
├── Services/
│   └── PedidoService.cs                # Serviço com acesso direto ao DbContext e projeção DTO
├── Controllers/
│   └── PedidosController.cs            # Controller permitindo comparar ambos os fluxos
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml
│   └── Pedidos/
│       ├── Index.cshtml                # Dashboard comparativo de abordagens
│       └── Criar.cshtml                # Formulário com Tag Helpers
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5900
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)

Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/09-repository-pattern/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5900`).

Na interface, você poderá testar e inspecionar:
- **Consulta via Projeção Direta no DbContext:** O serviço consulta apenas as colunas necessárias e projeta em um `PedidoResumoDto` com `AsNoTracking()`.
- **Criação com Transação Atômica via Repositório de Domínio:** Demonstração do `IPedidoRepository` executando a validação e persistência do pedido com `context.Database.BeginTransactionAsync()`.
- **Comparativo Visual:** Demonstração do número de classes e linhas de código economizadas ao não criar camadas artificiais sobre o EF Core.

---

### Versão Legada (.NET Framework 4.8.1)

1. Abra o arquivo `RepositoryPatternDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express na porta `5901`.
3. Acesse no navegador:
   - `http://localhost:5901/`: Listagem de pedidos consumindo o `GenericRepository<Pedido>` através do `UnitOfWork`.
   - `http://localhost:5901/pedidos/criar`: Criação de novo pedido.

Para compilar via terminal (requer ambiente Windows com MSBuild):
```cmd
msbuild concepts\09-repository-pattern\framework\RepositoryPatternDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A redundância do repositório genérico:** Observe o arquivo [GenericRepository.cs](file:///c:/GITHUB/dotnet-modernization-lab/concepts/09-repository-pattern/framework/Repositories/GenericRepository.cs). Veja como cada método simplesmente repassa a chamada para o `DbSet` interno, adicionando complexidade sem entregar regra de negócio.
2. **Projeções Diretas vs Carregamento Completo:** Veja em [PedidoService.cs](file:///c:/GITHUB/dotnet-modernization-lab/concepts/09-repository-pattern/net10/Services/PedidoService.cs) no .NET 10 como o LINQ projeta diretamente para o record `PedidoResumoDto`, trazendo do banco exatamente os dados necessários sem transportar entidades inteiras para a memória.
3. **Repositório de Domínio Real:** Compare a interface [IPedidoRepository.cs](file:///c:/GITHUB/dotnet-modernization-lab/concepts/09-repository-pattern/net10/Repositories/IPedidoRepository.cs) com o repositório genérico do legado. O repositório moderno expressa intenções de negócio claras (`CriarPedidoComItensAsync`, `ObterRelatorioPorPeriodoAsync`) em vez de simples operações genéricas.
