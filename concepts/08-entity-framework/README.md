# Lab 08: Entity Framework (EF 6 vs EF Core 10)

> Comparativo prático entre a persistência de dados com o clássico **Entity Framework 6** no .NET Framework 4.8.1 (configuração via Web.config, inicializadores de banco herdados, ausência de batching nativo em SaveChanges, consultas com joins pesados e necessidade de carregar entidades em memória para atualizações) e os recursos modernos do **Entity Framework Core 10** no .NET 10 (AddDbContext com injeção de dependência, batching automático de comandos SQL, AsNoTracking otimizado, AsSplitQuery e operações diretas em massa com ExecuteUpdateAsync e ExecuteDeleteAsync).

---

## O que é este conceito?

O **Entity Framework (EF)** é o ORM (Object-Relational Mapper) oficial da Microsoft para o ecossistema .NET. Ele abstrai o acesso a bancos de dados relacionais mapeando tabelas para classes de entidade e permitindo consultas expressas em LINQ.

- No **.NET Framework (com Entity Framework 6.x)**:
  - A configuração de conexão ficava amarrada ao arquivo `Web.config` na seção `<connectionStrings>`.
  - O ciclo de vida do `DbContext` era manual ou gerido por containers de injeção externos, já que não havia suporte nativo do framework.
  - A criação e evolução da base dependiam de classes derivadas de `Database.SetInitializer` (como `CreateDatabaseIfNotExists` ou `DropCreateDatabaseIfModelChanges`) ou de scripts executados exclusivamente pelo Package Manager Console do Visual Studio via comandos PowerShell (`Enable-Migrations`, `Add-Migration`).
  - O método `SaveChanges()` executava cada operação de inclusão ou alteração em comandos SQL isolados, gerando múltiplas idas e vindas à rede (roundtrips).
  - Para alterar ou excluir múltiplos registros, era obrigatório consultar todos os registros para a memória da aplicação, iterar com um loop `foreach`, alterar as propriedades e chamar `SaveChanges()`.
- No **.NET 10 (com Entity Framework Core 10)**:
  - O `DbContext` é configurado diretamente no fluxo de inicialização via `builder.Services.AddDbContext<AppDbContext>()`, integrando-se nativamente ao sistema de injeção de dependências e lendo opções de `appsettings.json`.
  - O `SaveChangesAsync()` agrupa automaticamente múltiplos comandos `INSERT`, `UPDATE` e `DELETE` em lotes (batching), reduzindo drasticamente o tempo de comunicação com o banco.
  - Operações em massa são executadas diretamente no servidor de banco de dados via `ExecuteUpdateAsync` e `ExecuteDeleteAsync`, sem necessidade de carregar entidades na memória ou de rastreá-las no Change Tracker.
  - O recurso `AsSplitQuery()` permite dividir consultas com múltiplos `Include` em queries SQL separadas, eliminando o problema de explosão cartesiana em joins.
  - As ferramentas de linha de comando (`dotnet ef`) são multiplataforma e podem ser executadas em Linux, macOS ou contêineres Docker.

---

## Por que mudou?

1. **Performance e Redução de Roundtrips (Batching):** No EF 6, salvar 100 entidades gerava 100 instruções SQL enviadas individualmente ao banco de dados. No EF Core, o compilador de consultas empacota essas operações em um lote único, gerando ganhos substanciais de latência e consumo de rede.
2. **Operações em Massa sem Custo de Memória:** O fluxo legado de buscar centenas de registros para a RAM da aplicação apenas para alterar um status gerava consumo excessivo de memória e lentidão de Garbage Collector. Métodos como `ExecuteUpdateAsync` traduzem a instrução diretamente para um comando `UPDATE ... WHERE ...` no SQL.
3. **Prevenção de Explosão Cartesiana com `AsSplitQuery`:** No EF 6, fazer `Include` em múltiplas coleções filhas resultava em uma única query gigantesca com vários `LEFT JOIN`, duplicando linhas e transferindo megabytes de dados redundantes. O EF Core permite que cada coleção seja buscada em sua própria consulta focada.
4. **Ciclo de Vida Limpo e Injeção Nativa:** No .NET moderno, o `DbContext` recebe seu ciclo de vida (`Scoped` por padrão) gerenciado de ponta a ponta pelo container de injeção de dependência do ASP.NET Core, evitando vazamento de conexões ou instâncias estáticas perigosas.
5. **Multiplataforma e Independência de IDE:** Enquanto as migrações do EF 6 exigiam o PowerShell do Visual Studio no Windows, a CLI do EF Core opera em qualquer terminal e sistema operacional.

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (EF 6) | .NET 10 (EF Core 10) |
|---|---|---|
| **Registro e Injeção** | Manual ou via factories no controller | Nativo via `builder.Services.AddDbContext<AppDbContext>()` |
| **Origem da Conexão** | `Web.config` via `ConfigurationManager` | `appsettings.json` via `IConfiguration` |
| **Criação Inicial do Banco** | `Database.SetInitializer` estático | `context.Database.EnsureCreatedAsync()` ou `MigrateAsync()` |
| **Persistência em Lote (Batching)** | Não suportado (um roundtrip por entidade em `SaveChanges`) | Nativo e automático em `SaveChangesAsync` |
| **Atualização em Massa (Bulk Update)** | Exige carregar entidades na memória e iterar em loop | Direto no banco via `ExecuteUpdateAsync` sem carregar na memória |
| **Exclusão em Massa (Bulk Delete)** | Exige buscar entidades e chamar `RemoveRange` + `SaveChanges` | Direto no banco via `ExecuteDeleteAsync` |
| **Consultas com Múltiplos Includes** | Single Query forçada (risco de explosão cartesiana de joins) | Suporte declarativo a `AsSplitQuery()` |
| **Rastreamento de Entidades** | `AsNoTracking()` básico | `AsNoTracking()` altamente otimizado com menor alocação |
| **Ferramentas de Migração** | Package Manager Console no Visual Studio (Windows) | CLI multiplataforma (`dotnet ef`) |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── EntityFrameworkDemo.csproj          # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Dependências: EntityFramework 6.4.4 e MVC 5
├── Web.config                          # Configuração de connectionStrings e provider do EF 6
├── Global.asax                         # Inicialização da aplicação
├── Global.asax.cs                      # Registro do inicializador do EF 6 (Database.SetInitializer)
├── App_Start/
│   └── RouteConfig.cs                  # Configuração de rotas convencionais
├── Models/
│   ├── Categoria.cs                    # Entidade Categoria
│   ├── Produto.cs                      # Entidade Produto com chave estrangeira
│   └── OperacaoResultadoViewModel.cs   # ViewModel para métricas didáticas
├── Data/
│   ├── AppDbContext.cs                 # DbContext clássico herdando de System.Data.Entity.DbContext
│   └── DbInitializer.cs                # Inicializador herdando de CreateDatabaseIfNotExists
├── Controllers/
│   └── ProdutosController.cs           # Ações de listagem, inserção e teste de bulk legado
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                      # Configuração do Razor 3
│   ├── Shared/_Layout.cshtml
│   └── Produtos/
│       ├── Index.cshtml                # Listagem e painel de ações do EF 6
│       └── Criar.cshtml                # Formulário de cadastro
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── EntityFrameworkDemo.csproj          # SDK-style visando net10.0 com EF Core Sqlite
├── Program.cs                          # Configuração do AddDbContext e inicialização do banco
├── appsettings.json                    # Connection string SQLite
├── Models/
│   ├── Categoria.cs                    # Entidade Categoria
│   ├── Produto.cs                      # Entidade Produto com relacionamentos
│   └── OperacaoResultadoViewModel.cs   # ViewModel para métricas didáticas
├── Data/
│   ├── AppDbContext.cs                 # DbContext moderno herdando de Microsoft.EntityFrameworkCore.DbContext
│   └── DbInitializer.cs                # Inicialização assíncrona com EnsureCreatedAsync
├── Controllers/
│   └── ProdutosController.cs           # Ações com AsNoTracking, AsSplitQuery e ExecuteUpdateAsync
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/_Layout.cshtml
│   └── Produtos/
│       ├── Index.cshtml                # Listagem e painel de operações modernas do EF Core
│       └── Criar.cshtml                # Formulário com Tag Helpers
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5800
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)

Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/08-entity-framework/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5800`).

Operações interativas disponíveis na tela:
- **Listagem Otimizada:** Exibe produtos com suas categorias utilizando `AsNoTracking()` e `AsSplitQuery()`.
- **Inserção em Lote com Batching:** Insere múltiplos produtos em um único comando SQL empacotado pelo EF Core.
- **Atualização em Massa Direta (Bulk Update):** Reajusta os preços de uma categoria inteira diretamente no banco via `ExecuteUpdateAsync` sem carregar as entidades para a memória.
- **Exclusão em Massa Direta (Bulk Delete):** Remove itens inativos diretamente via `ExecuteDeleteAsync`.

---

### Versão Legada (.NET Framework 4.8.1)

1. Abra o arquivo `EntityFrameworkDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express na porta `5801`.
3. Acesse no navegador:
   - `http://localhost:5801/`: Listagem MVC conectada ao EF 6.
   - `http://localhost:5801/produtos/criar`: Cadastro de novo produto.

Para compilar via terminal (requer ambiente Windows com MSBuild):
```cmd
msbuild concepts\08-entity-framework\framework\EntityFrameworkDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A diferença no registro de banco:** Compare como o `AppDbContext.cs` do .NET 10 recebe `DbContextOptions<AppDbContext>` e se registra no container de DI via `AddDbContext`, enquanto no EF 6 o contexto precisava invocar `base("name=DefaultConnection")` e ler diretamente do `Web.config`.
2. **Atualizações em massa:** No controller do EF Core, observe o uso de `await _context.Produtos.Where(...).ExecuteUpdateAsync(...)`. Note como essa chamada gera um único comando `UPDATE` no SQL Server ou SQLite, sem instanciar entidades em memória, ao contrário do loop manual obrigatório no EF 6.
3. **A separação de consultas com `AsSplitQuery`:** Veja como o EF Core permite evitar a explosão cartesiana ao carregar entidades relacionadas complexas.
