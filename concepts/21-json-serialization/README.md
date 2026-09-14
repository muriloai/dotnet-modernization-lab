# Conceito 21: Serialização, Cache e Desempenho - Serialização JSON

Este laboratório apresenta uma análise técnica e comparativa das soluções de serialização e desserialização JSON entre o ecossistema clássico do **.NET Framework 4.8.1** e a arquitetura moderna do **.NET 10**.

---

## 1. Contexto e Motivação

Durante quase duas décadas, a manipulação de JSON no .NET Framework enfrentou desafios estruturais:
1. **Ferramental nativo defasado:** O `JavaScriptSerializer` (do ASP.NET AJAX) e o `DataContractJsonSerializer` (do WCF) apresentavam formatos obsoletos de data (como `/Date(1534940000000)/`), desempenho insatisfatório e suporte limitado a tipos modernos.
2. **Dependência de biblioteca externa (Newtonsoft.Json / Json.NET):** O Json.NET tornou-se o padrão incontestável da indústria, vindo pré-instalado no ASP.NET MVC e Web API 2. Apesar de sua riqueza de recursos, sua arquitetura dependia extensivamente de reflexão em tempo de execução, alocações pesadas de strings na memória gerenciada e boxing de tipos primitivos.
3. **Riscos de segurança com TypeNameHandling:** A deserialização polimórfica no Json.NET frequentemente utilizava `TypeNameHandling.Auto` ou `All`, abrindo brechas para ataques de execução remota de código (RCE) via desserialização insegura de tipos arbitrários.
4. **Ausência de streaming de baixo nível:** Carregar arquivos JSON volumosos exigia buffers em memória contendo a string inteira, provocando picos de consumo e retenção no Large Object Heap (LOH).

No **.NET 10**, a infraestrutura de serialização foi remodelada através de `System.Text.Json`:
- **Operação Direta sobre UTF-8:** Os motores de baixo nível `Utf8JsonReader` e `Utf8JsonWriter` processam diretamente sequências de bytes (`ReadOnlySpan<byte>`), eliminando a etapa intermediária de conversão para strings no heap.
- **Source Generators para JSON (`JsonSerializerContext`):** A lógica de serialização e metadados de reflexão são gerados durante a compilação. Essa abordagem elimina a reflexão em tempo de execução, reduz o tempo de inicialização (cold start) a zero, viabiliza o uso de Native AOT e trimming, e reduz drasticamente o consumo de memória.
- **Polimorfismo Seguro e Tipado:** A especificação `[JsonPolymorphic]` combinada com `[JsonDerivedType]` permite criar discriminadores nomeados (como `"$tipo": "pix"`) com validação estrita, sem expor nomes de classes ou assemblies do .NET.
- **Streaming Assíncrono com `IAsyncEnumerable<T>`:** Permite transmitir ou consumir coleções JSON imensas sob demanda, fragmento por fragmento, mantendo a pegada de memória estável e constante.

---

## 2. Comparativo Técnico Direto

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Biblioteca Padrão** | Externa (`Newtonsoft.Json` / Json.NET) ou obsoleta (`JavaScriptSerializer`) | Nativa da BCL (`System.Text.Json`) |
| **Processamento de Bytes** | Converte bytes para `string` intermediária antes de desserializar | Lê e escreve diretamente em buffers UTF-8 (`ReadOnlySpan<byte>`) |
| **Geração de Código (Source Gen)** | Inexistente; depende obrigatoriamente de reflexão em tempo de execução | Nativa com `JsonSerializerContext` e atributo `[JsonSerializable]` |
| **Compatibilidade com Native AOT** | Incompatível devido à reflexão dinâmica não analisável | 100% compatível quando utilizado com Source Generators |
| **Polimorfismo** | Inseguro via `TypeNameHandling` (expondo nomes de tipos C#) | Declarativo e seguro via `[JsonPolymorphic]` e discriminadores explícitos |
| **Streaming de Coleções** | Carrega todo o array JSON na memória em objetos `JArray` | Suporte nativo a `IAsyncEnumerable<T>` com entrega sob demanda |
| **Imutabilidade e Records** | Suporte imperfeito a construtores com parâmetros posicionais | Suporte nativo a `record`, propriedades `init` e tipos imutáveis |
| **Consumo de Memória (Heap)** | Elevado devido a strings temporárias e nós de árvore DOM | Otimizado para alocação mínima ou zero no caminho crítico |

---

## 3. O Modelo Legado (.NET Framework 4.8.1)

No modelo tradicional, o desenvolvedor recorria a `Newtonsoft.Json`:

```csharp
// Exemplo classico de serializacao reflexiva no legado
var settings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    DateFormatHandling = DateFormatHandling.IsoDateFormat
};

// Gera string completa no heap
string jsonTexto = JsonConvert.SerializeObject(listaProdutos, settings);

// Desserializacao com inspecao em tempo de execucao
var retorno = JsonConvert.DeserializeObject<List<ProdutoLegado>>(jsonTexto);
```

Limitações deste modelo:
1. **Pressão no Garbage Collector:** Para cada requisição de API com payload de 5 MB, o servidor alocava strings de 5 MB somadas às estruturas internas de nós reflexivos do Json.NET, provocando frequentes coletas de lixo de Geração 2 e fragmentação do LOH.
2. **Insegurança em polimorfismo:** Para deserializar subclasses, ativava-se `TypeNameHandling.Objects`, permitindo que um invasor enviasse `$type: "System.Diagnostics.Process, System"` no payload e executasse comandos no servidor.

---

## 4. O Modelo Moderno (.NET 10)

No .NET 10, o processamento ocorre via Source Generators e anotações declarativas seguras.

### Polimorfismo Seguro com Discriminadores
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$tipo")]
[JsonDerivedType(typeof(PagamentoPix), "pix")]
[JsonDerivedType(typeof(PagamentoCartao), "cartao")]
public abstract class Pagamento
{
    public decimal Valor { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}

public class PagamentoPix : Pagamento
{
    public string ChavePix { get; set; } = string.Empty;
}

public class PagamentoCartao : Pagamento
{
    public string NumeroMascarado { get; set; } = string.Empty;
    public int Parcelas { get; set; }
}
```

### Source Generator em Tempo de Compilação (`CatalogoJsonContext.cs`)
```csharp
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<ProdutoCatalogo>))]
[JsonSerializable(typeof(Pagamento))]
public partial class CatalogoJsonContext : JsonSerializerContext
{
}
```

O compilador C# gera delegates especialistas sem reflexão:
```csharp
// Serializacao direta sem reflexao utilizando o contexto gerado
byte[] bytesUtf8 = JsonSerializer.SerializeToUtf8Bytes(
    produtos,
    CatalogoJsonContext.Default.ListProdutoCatalogo);
```

### Streaming Assíncrono com `IAsyncEnumerable<T>`
```csharp
[HttpGet("streaming-catalogo")]
public async IAsyncEnumerable<ProdutoCatalogo> ObterCatalogoStreaming()
{
    for (int i = 1; i <= 50; i++)
    {
        await Task.Delay(50); // Simula leitura fracionada do banco
        yield return new ProdutoCatalogo(i, $"Produto #{i}", 99.90m * i);
    }
}
```

---

## 5. Estrutura deste Laboratório

```text
concepts/21-json-serialization/
├── README.md
├── framework/
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Models/
│   │   ├── PagamentoLegado.cs
│   │   └── ProdutoLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   └── SerializadorLegadoService.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── Global.asax
│   ├── Global.asax.cs
│   ├── JsonSerializationDemo.csproj
│   ├── packages.config
│   └── Web.config
└── net10/
    ├── Contexts/
    │   └── CatalogoJsonContext.cs
    ├── Controllers/
    │   └── SerializacaoController.cs
    ├── Models/
    │   ├── MetricasSerializacao.cs
    │   ├── Pagamento.cs
    │   └── ProdutoCatalogo.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   └── SerializacaoBenchmarkService.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── site.css
    │   └── index.html
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── JsonSerializationDemo.csproj
    └── Program.cs
```

---

## 6. Como Executar os Projetos

### Executando o Projeto Moderno (.NET 10)
1. Abra o terminal na pasta `concepts/21-json-serialization/net10`.
2. Execute o comando:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador em `http://localhost:7100`.
4. Teste a deserialização polimórfica segura, execute o benchmark de comparação entre reflexão e Source Generator, e visualize o consumo contínuo via streaming assíncrono.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra a pasta `concepts/21-json-serialization/framework` no Visual Studio 2022 em um ambiente Windows com carga ASP.NET.
2. Inicie o projeto através do IIS Express na porta `7101`.
3. Acesse `http://localhost:7101` para inspecionar as limitações de alocação de memória do `JavaScriptSerializer` e do `Newtonsoft.Json`.
