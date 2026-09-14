# Conceito 23: Otimizações de Desempenho

Este laboratório encerra o módulo de **Serialização, Cache e Desempenho**, analisando os recursos modernos de alta performance do **.NET 10** em comparação com as práticas tradicionais de gerenciamento de memória no **.NET Framework 4.8.1**.

---

## 1. Contexto e Motivação

Durante anos, desenvolvedores em .NET Framework aceitavam alocações frequentes no Garbage Collector (GC) como parte natural do desenvolvimento em C#. Métodos como `string.Substring`, `string.Split`, concatenações sucessivas e alocações de vetores de bytes (`new byte[4096]`) eram rotineiros.

### No .NET Framework 4.8.1 (Legado):
- **Alocações Excessivas em Heap:** Cada chamada a `string.Substring` ou `string.Split` aloca novas instâncias de string e novos arrays na Heap gerenciada. Em aplicações com milhares de requisições por segundo, isso satura a Geração 0 (Gen 0) e dispara coletas de lixo frequentes.
- **Fragmentação da Large Object Heap (LOH):** Buffers de I/O maiores que 85.000 bytes são alocados diretamente na LOH. No .NET Framework, a LOH raramente sofre compactação, fragmentando a memória virtual e podendo provocar `OutOfMemoryException` mesmo com memória física disponível.
- **Boxing e Unboxing:** O uso de coleções ou APIs que aceitam `object` força tipos por valor a sofrerem encapsulamento em objetos heap, gerando pressão adicional no coletor de lixo.
- **Ausência de Tipos Contíguos Seguros:** Não havia representação segura para manipular fatias de memória da pilha (stack) ou buffers nativos sem recorrer a ponteiros não gerenciados (`unsafe`).

### No .NET 10 (Moderno):
- **Span<T> e ReadOnlySpan<T> (Zero-Allocation):** `Span<T>` é uma `ref struct` que reside exclusivamente na pilha de execução. Permite fatiar strings, vetores e memória não gerenciada sem alocar nenhum byte adicional na Heap. O fatiamento `ReadOnlySpan<char>` de uma string não cria novos objetos.
- **ArrayPool<T>.Shared:** Pool global de arrays que permite alugar (`Rent`) e devolver (`Return`) buffers reutilizáveis. Elimina a criação contínua de arrays temporários para I/O e streaming.
- **Formatação Direta em Span (ISpanFormattable):** Converte números, datas e identificadores diretamente em buffers de memória sem gerar strings intermediárias.
- **DATAS (Dynamic Adaptation To Application Sizes):** No .NET 10, o coletor de lixo adapta dinamicamente seu consumo de memória com base no tamanho e comportamento real da carga, mantendo latência previsível em contêineres e na nuvem.

---

## 2. Comparativo Técnico Estruturado

| Característica | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Fatiamento de Strings** | `string.Substring` cria uma nova string na Heap a cada chamada | `ReadOnlySpan<char>` fatia em pilha com 0 bytes alocados |
| **Parsing e Tokenização** | `string.Split` aloca array de strings + cada substring | `MemoryExtensions.EnumerateSplits` ou Span sem alocação |
| **Gerenciamento de Buffers** | `new byte[bufferSize]` contínuo, gerando lixo na Gen 0 ou LOH | `ArrayPool<byte>.Shared.Rent/Return` reciclando arrays |
| **Estruturas de Pilha** | Limitado a ponteiros `unsafe` e `stackalloc` restrito | `Span<T>`, `ReadOnlySpan<T>` com segurança de tipo e memória |
| **Conversão de Dados** | `int.ToString()` cria string na heap | `int.TryFormat` escreve diretamente em Span existente |
| **Coletor de Lixo** | Workstation e Server GC tradicionais do Windows | DATAS, Compactação sob demanda, suporte a contêineres |

---

## 3. Estrutura dos Projetos

```text
concepts/23-performance/
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
│   │   └── ResultadoBenchmarkLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   └── ParserLogLegadoService.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── PerformanceDemo.csproj
│   ├── Web.config
│   └── packages.config
└── net10/                                  # Projeto Moderno (.NET 10)
    ├── Controllers/
    │   └── PerformanceController.cs
    ├── Models/
    │   └── ResultadoBenchmark.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   └── ParserLogModernoService.cs
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── PerformanceDemo.csproj
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
| `net10/` | .NET 10 | `http://localhost:7300` | Kestrel |
| `framework/` | .NET Framework 4.8.1 | `http://localhost:7301` | IIS Express |

---

## 5. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)
```bash
cd concepts/23-performance/net10
dotnet run
```
Acesse `http://localhost:7300` para interagir com o laboratório de benchmark de parsing e reciclagem de buffers com `Span<T>` e `ArrayPool<T>`.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra `PerformanceDemo.csproj` no Visual Studio 2022.
2. Inicie no IIS Express na porta `7301`.
3. Compare as métricas de tempo e alocação de memória geradas pelos métodos legados baseados em `Substring` e `new byte[]`.
