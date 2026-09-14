# Conceito 22: Estratégias de Cache

Este laboratório compara a evolução das estratégias de cache entre o **.NET Framework 4.8.1** e o **.NET 10**, demonstrando cache em memória (`IMemoryCache`), mitigação de cache stampede, abstração unificada de cache distribuído (`IDistributedCache`) e o middleware moderno de Output Caching com invalidação semântica orientada a tags (`IOutputCacheStore`).

---

## 1. Contexto e Motivação

O armazenamento temporário em cache é uma das técnicas mais eficazes para reduzir latência, economizar recursos computacionais e proteger bancos de dados sob alta concorrência. Contudo, a forma como o cache é manipulado mudou drasticamente ao longo da evolução do ecossistema .NET.

### No .NET Framework 4.8.1 (Legado):
- **Acoplamento com IIS:** O cache em memória tradicionalmente utilizava `System.Web.HttpContext.Current.Cache` ou `System.Web.Caching.Cache`. Ambas as classes são fortemente dependentes do pipeline do ASP.NET e do IIS, inviabilizando testes unitários sem mocks complexos do contexto web.
- **MemoryCache Estático:** A introdução do `System.Runtime.Caching.MemoryCache.Default` trouxe uma opção desvinculada do ASP.NET, mas estruturada como um singleton estático. Isso impedia a injeção de dependências controlada e dependia de políticas de expiração baseadas em limites de memória física do Windows, frequentemente falhas em contêineres.
- **Vulnerabilidade a Cache Stampede (Dogpiling):** Quando uma chave quente expirava sob concorrência de centenas de requisições por segundo, todas as threads identificavam o cache miss simultaneamente e consultavam o banco de dados de uma só vez, degradando ou derrubando a fonte de dados. O tratamento exigia blocos manuais de `lock` ou `Monitor`, propensos a deadlocks.
- **Ausência de Abstração para Cache Distribuído:** A BCL não fornecia uma interface uniforme para cache distribuído. Equipes precisavam criar adaptadores manuais para Redis, Memcached ou AppFabric, gerando acoplamento direto com bibliotecas de terceiros.
- **OutputCache Rígido:** O atributo `[OutputCache]` do ASP.NET MVC armazenava respostas HTML em cache, mas sua invalidação exigia esperar o tempo limite ou invocar `HttpResponse.RemoveOutputCacheItem(urlExata)`. Não havia qualquer suporte a invalidação semântica por tags ou entidades de domínio.

### No .NET 10 (Moderno):
- **IMemoryCache via Injeção de Dependências:** Registrado através de `builder.Services.AddMemoryCache()`, o `IMemoryCache` é injetado diretamente nos serviços que dele necessitam. Suporta expirações deslizantes (`SlidingExpiration`), absolutas (`AbsoluteExpirationRelativeToNow`), limites de tamanho por entrada (`SizeLimit`) e callbacks de expulsão (`RegisterPostEvictionCallback`).
- **Resolução de Cache Stampede:** O padrão `GetOrCreateAsync` combinado com semáforos assíncronos (`SemaphoreSlim`) garante que apenas uma única requisição execute a computação pesada ou chamada ao banco de dados no momento da expiração da chave, enquanto as demais requisições aguardam de forma assíncrona o preenchimento do valor.
- **IDistributedCache Padronizado:** Interface padrão da BCL (`Microsoft.Extensions.Caching.Distributed.IDistributedCache`) permitindo alternar de forma transparente entre cache em memória distribuído para desenvolvimento (`AddDistributedMemoryCache`), Redis (`AddStackExchangeRedisCache`) ou SQL Server para produção, sem alterar uma única linha do código de negócio.
- **Output Caching com Tag-Based Eviction:** O middleware de Output Caching (`AddOutputCache`, `UseOutputCache`) oferece bloqueio contra stampede nativo e invalidação por tags. Ao associar tags de domínio a rotas cacheadas (`categoria-{categoria}`), qualquer alteração em um produto permite invocar `IOutputCacheStore.EvictByTagAsync("categoria-eletronicos", ct)`, invalidando imediatamente todas as rotas que dependiam daquela entidade, sem precisar saber a URL exata ou combinações de parâmetros da requisição.

---

## 2. Comparativo Técnico Estruturado

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Cache em Memória** | `System.Web.Caching.Cache` (estático/acoplado ao IIS) ou `System.Runtime.Caching.MemoryCache.Default` | `IMemoryCache` registrado via Injeção de Dependências com escopos bem definidos |
| **Testabilidade** | Difícil: requer instanciação de contextos HTTP ou singletons globais | Simples: interface `IMemoryCache` facilmente mockável em testes unitários |
| **Cache Stampede** | Sem tratamento nativo: requisições concorrentes atingem o banco simultaneamente ao expirar chave | Mitigado via `GetOrCreateAsync` ou `SemaphoreSlim` por chave, serializando chamadas concorrentes |
| **Controle de Memória** | Porcentagem da memória física do Windows ou contadores estáticos | `SizeLimit` configurável por entrada, garantindo tetos previsíveis em contêineres |
| **Cache Distribuído** | Inexistente na BCL: cada fornecedor impunha sua própria API proprietária | Interface padronizada `IDistributedCache` nativa |
| **Cache de Resposta/Página** | `[OutputCache]` MVC com invalidação frágil por URL completa | Middleware de Output Caching com suporte a políticas e invalidação por tags (`EvictByTagAsync`) |
| **Bloqueio de Saída HTTP** | Bloqueio simples por rota; risco de travamento de threads em requisições concorrentes | Bloqueio assíncrono interno integrado no middleware de Output Caching |

---

## 3. Estrutura dos Projetos

```text
concepts/22-caching/
├── README.md
├── framework/                              # Projeto Legado (.NET Framework 4.8.1)
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Global.asax
│   │   └── Global.asax.cs
│   ├── Models/
│   │   └── ProdutoLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   └── CatalogoLegadoService.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── CachingDemo.csproj
│   ├── Web.config
│   └── packages.config
└── net10/                                  # Projeto Moderno (.NET 10)
    ├── Controllers/
    │   └── CacheController.cs
    ├── Models/
    │   ├── MetricasStampede.cs
    │   └── Produto.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   ├── CatalogoService.cs
    │   └── ICatalogoService.cs
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── CachingDemo.csproj
    ├── Program.cs
    └── wwwroot/
        ├── css/
        │   └── site.css
        └── index.html
```

---

## 4. Portas dos Projetos

| Projeto | Runtime | Porta HTTP | Servidor |
| :--- | :--- | :--- | :--- |
| `net10/` | .NET 10 | `http://localhost:7200` | Kestrel |
| `framework/` | .NET Framework 4.8.1 | `http://localhost:7201` | IIS Express |

---

## 5. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)
No terminal, acesse a pasta do projeto moderno e execute:

```bash
cd concepts/22-caching/net10
dotnet run
```

Abra o navegador no endereço `http://localhost:7200`.

O painel interativo permite:
1. **Simulador de Cache Stampede:** Disparar 10 requisições concorrentes sem trava e com trava, observando o número de execuções reais no banco simulado (10 acessos vs 1 único acesso compartilhado).
2. **Output Caching com Tags:** Consultar respostas cacheadas por categoria com carimbo de tempo do servidor e invalidar seletivamente a categoria alterada via `IOutputCacheStore.EvictByTagAsync`.
3. **Cache Distribuído:** Obter e renovar objetos complexos armazenados via `IDistributedCache`.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra o arquivo `CachingDemo.csproj` no Visual Studio 2022.
2. Inicie a execução (`Ctrl + F5`) para carregar a aplicação no IIS Express na porta `7201`.
3. Teste o comportamento do `HttpRuntime.Cache` e do `[OutputCache]`, observando a falta de suporte a tags e a dependência do contexto estático do IIS.
