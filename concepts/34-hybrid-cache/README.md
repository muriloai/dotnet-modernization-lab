# Conceito 34: HybridCache no .NET 10 vs Caching Legado no .NET Framework 4.8.1

Este laboratório demonstra a evolução das estratégias de armazenamento em cache no ecossistema .NET, contrastando o modelo clássico e fragmentado do .NET Framework 4.8.1 (`System.Runtime.Caching.MemoryCache` e `HttpContext.Current.Cache`) com a abstração unificada de alta performance **HybridCache** (`Microsoft.Extensions.Caching.Hybrid`), introduzida no .NET 9 e aprimorada no .NET 10.

---

## 1. Visão Geral e Contexto Prático

O armazenamento em cache é essencial para reduzir a latência de requisições e diminuir a carga em bancos de dados relacionais e APIs externas. No entanto, gerenciar cache em aplicações corporativas impõe três grandes desafios técnicos:

1. **Arquitetura Multi-Camada (L1 vs L2):** Manter itens frequentemente acessados na memória do processo local (L1) para leitura quase instantânea sem custo de serialização, combinando com uma camada distribuída compartilhada (L2, como Redis ou Garnet) para consistência entre instâncias.
2. **Proteção contra Cache Stampede (Thundering Herd):** Quando um item quente expira ou ainda não foi gerado, centenas de requisições simultâneas sofrem Cache Miss no mesmo milissegundo. Sem uma proteção atômica, todas as requisições disparam a mesma consulta pesada ao banco em paralelo, causando picos de CPU, contenção de conexões e indisponibilidade.
3. **Invalidação Declarativa por Tags:** Ao atualizar uma entidade mestre (como uma categoria de produtos), é necessário invalidar em lote dezenas ou centenas de chaves associadas sem precisar conhecer todas as chaves individualmente de antemão.

---

## 2. O Modelo Legado no .NET Framework 4.8.1

No .NET Framework 4.8.1, as soluções disponíveis apresentavam deficiências estruturais:

* **Abstrações Desconectadas:** O desenvolvedor usava `System.Web.Caching.Cache` ou `System.Runtime.Caching.MemoryCache`. Para incluir Redis ou SQL Server, era necessário escrever código manual complexo para coordenar leitura no cache local, depois no distribuído, depois no banco, com risco contínuo de dados inconsistentes entre instâncias.
* **Ausência de Bloqueio contra Cache Stampede:** O método tradicional `cache.Get(key)` seguido de `if (cached == null) { ... cache.Set(key, val); }` não é atômico. Se 50 threads executarem simultaneamente, todas executarão o bloco interno. Tentar contornar isso com `lock` ou `SemaphoreSlim` frequentemente provocava deadlocks ou gargalos severos de contenção no IIS (`w3wp.exe`).
* **Falta de Invalidação por Tags:** Não existia o conceito de tags. Para remover todos os produtos de uma categoria, os desenvolvedores recorriam a varreduras completas no dicionário de chaves via LINQ (`MemoryCache.Default.Where(...)`), com complexidade O(N) e travamento de leitura.
* **Serialização Ineficiente:** A comunicação com camadas externas dependia de `BinaryFormatter` (hoje proibido por motivos de segurança) ou `Newtonsoft.Json`, gerando alto volume de alocações e sobrecarga no Garbage Collector.

---

## 3. A Solução Moderna: HybridCache no .NET 10

A biblioteca `Microsoft.Extensions.Caching.Hybrid` padroniza as melhores práticas de engenharia de software diretamente na plataforma:

* **Orquestração Transparente L1/L2:** As leituras consultam primeiro a memória em processo (L1). Se houver miss, consultam a camada distribuída (L2) e, somente se necessário, executam o delegate de fábrica (`factory`). O resultado é retroalimentado automaticamente em ambas as camadas.
* **Proteção Nativa contra Cache Stampede:** Ao utilizar `GetOrCreateAsync(key, factory)`, a plataforma garante bloqueio atômico baseado na chave. Se 100 requisições simultâneas solicitarem a mesma chave ausente, **apenas 1 execução da factory ocorre**. As outras 99 aguardam de forma não bloqueante e recebem o resultado gerado pela primeira.
* **Invalidação Seletiva por Tags:** Permite vincular chaves a uma ou mais tags no momento da inserção. Com uma única chamada a `RemoveByTagAsync("category:hardware")`, todas as chaves correspondentes são invalidadas instantaneamente.
* **Serialização e Desempenho Otimizados:** O acesso a itens em L1 não aloca memória de serialização (armazena referências diretas de objetos C#). Na camada L2, utiliza serializadores compactos com reutilização de buffers via `ArrayPool<byte>`.

---

## 4. Comparativo de Implementação

### Consulta no Legado (.NET Framework 4.8.1)

```csharp
// System.Runtime.Caching no .NET Framework 4.8.1
var cached = MemoryCache.Default.Get(cacheKey) as List<ProdutoLegadoDto>;
if (cached == null)
{
    // Risco critico de Cache Stampede se multiplas threads chegarem aqui simultaneamente!
    cached = ObterProdutosDoBanco(categoria);
    var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(2) };
    MemoryCache.Default.Set(cacheKey, cached, policy);
}
```

### Consulta no Moderno (.NET 10 com HybridCache)

```csharp
// Microsoft.Extensions.Caching.Hybrid no .NET 10
var produtos = await cache.GetOrCreateAsync(
    $"catalog:products:{categoria}",
    async cancelToken => await catalogService.GetProductsFromDatabaseAsync(categoria, cancelToken),
    tags: new[] { "products", $"category:{categoria}" },
    cancellationToken: ct
);
```

---

## 5. Tabela Comparativa Estrutural

| Característica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Biblioteca Principal** | `System.Runtime.Caching` ou `System.Web.Caching` | `Microsoft.Extensions.Caching.Hybrid` |
| **Camadas Suportadas** | Isolada (apenas memória local em processo) | Unificada (L1 memória em processo + L2 distribuído) |
| **Cache Stampede** | Vulnerável por padrão; exigia sincronização manual frágil | Protegido nativamente por bloqueio atômico por chave |
| **Invalidação em Lote** | Inexistente; requeria varredura manual de chaves O(N) | Nativa via Tags declarativas (`RemoveByTagAsync`) |
| **Alocação de Memória** | Alta sobrecarga de boxing e alocações de delegates | Zero alocação para hits em L1 e reaproveitamento de buffers |
| **Compatibilidade Cross-Platform** | Apenas Windows (acoplado ao w3wp e IIS) | Linux, macOS e Windows em containers leves |

---

## 6. Estrutura dos Projetos

```text
concepts/34-hybrid-cache/
|-- README.md
|-- framework/
|   |-- App_Start/
|   |   `-- RouteConfig.cs
|   |-- Content/
|   |   `-- Site.css
|   |-- Controllers/
|   |   `-- CacheLegadoController.cs
|   |-- Models/
|   |   `-- ProdutoLegadoDto.cs
|   |-- Views/
|   |   |-- CacheLegado/
|   |   |   `-- Index.cshtml
|   |   `-- Shared/
|   |       `-- _Layout.cshtml
|   |-- CacheLegadoDemo.csproj
|   |-- Global.asax
|   `-- Web.config
`-- net10/
    |-- Models/
    |   `-- ProductDto.cs
    |-- Properties/
    |   `-- launchSettings.json
    |-- Services/
    |   `-- ProductCatalogService.cs
    |-- wwwroot/
    |   |-- css/
    |   |   `-- site.css
    |   `-- index.html
    |-- HybridCacheDemo.csproj
    |-- Program.cs
    `-- appsettings.json
```

---

## 7. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/34-hybrid-cache/net10
   ```
2. Execute a aplicação:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   `http://localhost:8400`
4. Experimente os seguintes cenários no painel:
   * Realize a primeira consulta em uma categoria e note a latência de 200ms (Cache Miss no banco).
   * Repita a consulta imediatamente e observe o tempo reduzido para 0ms a 2ms (Cache Hit em L1).
   * Execute o teste de **Cache Stampede** com 10 ou 25 requisições concorrentes e confirme que a factory foi executada exatamente 1 vez.
   * Dispare a **Invalidação por Tag** e note que a próxima consulta reconstrói os dados de forma transparente.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra o arquivo `CacheLegadoDemo.csproj` no Visual Studio 2022.
2. Inicie a aplicação com IIS Express na porta configurada:
   `http://localhost:8401`
3. Dispare o teste de concorrência e comprove que múltiplas threads executam a busca no banco simultaneamente pela ausência de lock atômico.
