# Conceito 24: Evolução da Linguagem C# (7.3 vs 14)

Este laboratório compara a evolução expressiva e de produtividade da linguagem C#, contrastando a versão **C# 7.3** (a última suportada oficialmente no **.NET Framework 4.8.1**) com o **C# 14** no **.NET 10**.

---

## 1. Contexto e Motivação

Entre o C# 7.3 (lançado em 2018) e o C# 14 (em 2025/2026 com o .NET 10), a linguagem C# passou pela maior transformação de sua história. O código que antes exigia dezenas de linhas de cerimônia, validações defensivas e classes auxiliares prolixas agora pode ser expresso de forma concisa, segura e fortemente tipada.

### No .NET Framework 4.8.1 (C# 7.3):
- **Sintaxe Cerimonial e Aninhada:** Obrigatória a declaração de namespaces com chaves `{ }`, classe `Program`, método `static void Main(string[] args)` e blocos extensos de `using` repetidos em todos os arquivos de código.
- **Classes DTO Prolixas:** Para criar um objeto imutável de transferência de dados com igualdade por valor, o desenvolvedor precisava escrever construtores manuais, sobrecarregar `Equals`, `GetHashCode`, `ToString` e o operador `==`.
- **Pattern Matching Primitivo:** O C# 7.3 possuía apenas suporte básico a declarações de tipo com `is` e comandos `switch` tradicionais imperativos com `case` e `break`. Não existiam expressões switch (`switch expressions`), padrões posicionais, relacionais ou de propriedades.
- **Tratamento de Nulos Frágil:** Não havia *Nullable Reference Types* (`string?`). Qualquer referência podia ser nula em tempo de execução sem aviso do compilador, resultando em frequentes `NullReferenceException`.
- **Strings e Coleções Rígidas:** Para strings multilinhas ou JSON em código, usava-se `@""` exigindo escape duplo de aspas (`""`). A instanciação de vetores exigia `new int[] { 1, 2, 3 }` e não havia sintaxe unificada para combinar coleções.

### No .NET 10 (C# 14):
- **Redução de Cerimônia:** Top-level statements, file-scoped namespaces (`namespace MinhaEmpresa;`) e global usings eliminam até 4 níveis de indentação desnecessária e dezenas de linhas repetitivas.
- **Records e Primary Constructors:** A declaração de um DTO imutável com igualdade estrutural é feita em uma única linha (`public record Pedido(int Id, string Cliente, decimal Valor);`). Suporte a cópias não destrutivas via expressão `with`.
- **Pattern Matching Avançado:** Expressões switch concisas com padrões relacionais (`>= 1000 and <= 5000`), padrões de propriedades (`{ Status: "Aprovado", Itens.Count: > 0 }`), padrões combinados (`and`, `or`, `not`) e list patterns (`[primeiro, .., ultimo]`).
- **Raw String Literals e Collection Expressions:** Strings literais brutas com três ou mais aspas (`""" ... """`) preservam formatação exata e aspas internas sem caracteres de escape. Expressões de coleção (`[1, 2, ..outros]`) unificam a sintaxe de listas, arrays e spans.
- **Null Safety em Tempo de Compilação:** Sistema de tipos de referência anuláveis (`#nullable enable`) que analisa fluxo de controle e detecta possíveis erros de nulidade em tempo de compilação.

---

## 2. Comparativo Técnico Estruturado

| Recurso | C# 7.3 (.NET Framework 4.8.1) | C# 14 (.NET 10) |
| :--- | :--- | :--- |
| **Escopo de Namespace** | Em bloco aninhado com chaves `{ }` | File-scoped namespace (`namespace Nome;`) |
| **Ponto de Entrada** | `class Program { static void Main() { ... } }` | Top-level statements diretos no arquivo |
| **Tipos Imutáveis** | Classes prolixas com Equals/GetHashCode manuais | `record` e `record struct` com `with` expression |
| **Construtores de Classe** | Construtores cerimoniais com atribuição de campos | Primary Constructors diretos na assinatura da classe |
| **Pattern Matching** | Apenas switch imperativo clássico com `case` e `break` | Switch expressions, relational patterns, property patterns |
| **JSON e Multilinhas** | Verbatim strings `@""` com escape duplicado `""` | Raw String Literals `""" ... """` sem nenhum escape |
| **Criação de Coleções** | `new List<int> { 1, 2 }` e `new int[] { 1, 2 }` | Collection expressions `[1, 2, ..outros]` |
| **Segurança contra Nulos** | Apenas checagens manuais `if (obj == null)` | Nullable Reference Types com análise estática do compilador |

---

## 3. Estrutura dos Projetos

```text
concepts/24-csharp-evolution/
├── README.md
├── framework/                              # Projeto Legado (.NET Framework 4.8.1 - C# 7.3)
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Global.asax
│   │   └── Global.asax.cs
│   ├── Models/
│   │   ├── ClienteLegado.cs
│   │   └── PedidoLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   └── RegrasNegocioLegadoService.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── CSharpEvolutionDemo.csproj
│   ├── Web.config
│   └── packages.config
└── net10/                                  # Projeto Moderno (.NET 10 - C# 14)
    ├── Controllers/
    │   └── CSharpEvolutionController.cs
    ├── Models/
    │   ├── Cliente.cs
    │   └── Pedido.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   └── RegrasNegocioModernoService.cs
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── CSharpEvolutionDemo.csproj
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
| `net10/` | .NET 10 (C# 14) | `http://localhost:7400` | Kestrel |
| `framework/` | .NET Framework 4.8.1 (C# 7.3) | `http://localhost:7401` | IIS Express |

---

## 5. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)
```bash
cd concepts/24-csharp-evolution/net10
dotnet run
```
Acesse `http://localhost:7400` para experimentar:
1. Avaliação de descontos de pedidos via **Pattern Matching avançado** (relational + property patterns).
2. Criação e mutação não destrutiva de **Records** via expressão `with`.
3. Demonstração de **Raw String Literals** e **Collection Expressions**.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra `CSharpEvolutionDemo.csproj` no Visual Studio 2022.
2. Inicie com `Ctrl + F5` no IIS Express na porta `7401`.
3. Veja o código em C# 7.3 com if-else encadeados, classes com construtores manuais e tratamento de nulos sem auxílio do compilador.
