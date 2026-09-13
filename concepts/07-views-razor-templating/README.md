# Lab 07: Views, Razor e Templating

> Comparativo prático entre a renderização de telas no **.NET Framework 4.8.1** (Razor 3 clássico, HTML Helpers com objetos anônimos intrusivos, Child Actions síncronas que executavam todo o pipeline de controller e compilação tardia em runtime no IIS) e os recursos modernos do **.NET 10** (Tag Helpers com sintaxe HTML natural, View Components assíncronos e desacoplados, Razor Pages com modelo de código coeso e compilação antecipada via Razor SDK).

---

## O que é este conceito?

A camada de **Views e Templating** é responsável por gerar a interface visual HTML entregue ao usuário, combinando lógica de apresentação em C# com código HTML padrão.

- No **.NET Framework (MVC 3 a MVC 5)**:
  - A geração de elementos de formulário e links dependia de **HTML Helpers**, como `@Html.TextBoxFor(m => m.Nome, new { @class = "form-control" })`. Esses métodos misturavam sintaxe de código em meio às tags HTML, tornando o código poluído e dificultando o trabalho de designers e ferramentas frontend.
  - Para reutilizar pedaços de tela que dependiam de lógica ou dados do banco, utilizava-se **Child Actions** (`[ChildActionOnly]` e `@Html.Action()`). Isso exigia passar por todo o ciclo de vida de um controller, gerando custo desnecessário de processamento.
  - Toda página `.cshtml` precisava obrigatoriamente de uma classe Controller externa correspondente, mesmo quando a tela continha apenas formulários simples.
  - A compilação do Razor acontecia dinamicamente em tempo de execução no IIS, fazendo com que a primeira requisição sofresse atrasos perceptíveis.
- No **.NET 10**:
  - **Tag Helpers:** O Razor enriquece elementos HTML comuns com atributos semânticos, como `<input asp-for="Nome" class="form-control" />`. O HTML se mantém limpo e legível.
  - **View Components:** Substitutos diretos e modernos para Child Actions. São classes independentes de controllers, com suporte completo a injeção de dependência no construtor e métodos assíncronos (`InvokeAsync`).
  - **Razor Pages:** Para fluxos baseados em páginas e formulários, o modelo de Razor Pages organiza a tela (`.cshtml`) e seu modelo de código (`PageModel` no `.cshtml.cs`) lado a lado, sem a necessidade de controllers separados.
  - **Injeção Direta com `@inject`:** Serviços registrados no container de injeção de dependências podem ser injetados diretamente nas views usando a diretiva `@inject`.
  - **Compilação Antecipada:** As views são compiladas diretamente para o assembly da aplicação durante o comando `dotnet build`, identificando erros de sintaxe e tipos antes da execução.

---

## Por que mudou?

1. **Separação Limpa de Responsabilidades e Sintaxe Amigável:** Os HTML Helpers do MVC clássico forçavam o uso de objetos anônimos em C# para definir atributos HTML simples (`new { @class = "btn btn-primary", placeholder = "Digite seu nome" }`). Com as Tag Helpers do .NET moderno, os atributos HTML permanecem nativos e as tags continuam válidas para qualquer ferramenta de design ou linter HTML.
2. **Eliminação da Sobrecarga de Child Actions:** Chamar `@Html.Action()` no .NET Framework instanciava um controller inteiro, executava filtros de ação e criava um novo contexto de requisição filho. Os View Components do ASP.NET Core atuam diretamente sobre o modelo necessário, com execução assíncrona leve e sem dependência de controllers.
3. **Coesão com Razor Pages:** Em aplicações orientadas a páginas (como dashboards, formulários de cadastro e cadastros administrativos), ter que criar uma pasta em `Controllers/`, outra em `Views/` e outra em `Models/` para uma única tela gerava dispersão. O Razor Pages mantém a interface e os handlers de requisição (`OnGet`, `OnPost`) juntos de forma altamente coesa.
4. **Segurança de Tipos em Tempo de Compilação:** No modelo legado, um erro de digitação ou incompatibilidade de tipo em uma View muitas vezes só era descoberto quando a página era acessada no navegador pelo usuário final. No .NET moderno, o Razor SDK valida as views durante o build da aplicação.
5. **Injeção de Dependências Simplificada:** Em vez de depender de localizadores de serviços globais como `DependencyResolver.Current.GetService<T>()`, o Razor moderno aceita injeção declarativa no topo do arquivo via `@inject IMeuServico Servico`.

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Criação de Campos de Formulário** | HTML Helpers: `@Html.TextBoxFor(m => m.Nome, new { @class = "campo" })` | Tag Helpers: `<input asp-for="Nome" class="campo" />` |
| **Geração de Formulários** | `@using (Html.BeginForm("Criar", "Produtos", FormMethod.Post))` | `<form asp-controller="Produtos" asp-action="Criar" method="post">` |
| **Validação Visual** | `@Html.ValidationMessageFor(m => m.Nome)` | `<span asp-validation-for="Nome" class="text-danger"></span>` |
| **Componentes de Interface Reutilizáveis** | Child Actions: `[ChildActionOnly]` acionado por `@Html.Action()` | View Components: classes com `InvokeAsync` acionadas por `<vc:meu-componente />` |
| **Páginas Orientadas a Formulário** | Exigia criar Controller + View em diretórios separados | Suporte nativo a **Razor Pages** (`.cshtml` + `.cshtml.cs` com `PageModel`) |
| **Injeção de Serviços na View** | Manual via `DependencyResolver.Current.GetService<T>()` | Declarativa nativa com diretiva `@inject IMeuServico Servico` |
| **Momento da Compilação do Razor** | Em tempo de execução pelo IIS (ou via `MvcBuildViews` no MSBuild) | Em tempo de compilação diretamente pelo SDK do Razor (`dotnet build`) |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── ViewsAndRazorDemo.csproj             # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                     # Dependências: ASP.NET MVC 5 e Razor 3
├── Web.config                          # Configurações do runtime clássico
├── Global.asax                         # Inicialização da aplicação
├── Global.asax.cs                      # Registro de rotas MVC
├── App_Start/
│   └── RouteConfig.cs                  # Configuração de rotas convencionais
├── Models/
│   ├── ProdutoViewModel.cs             # Modelo do produto com DataAnnotations
│   └── ResumoCatalogoViewModel.cs      # Modelo consumido pela Child Action
├── Services/
│   ├── ICatalogoService.cs             # Contrato de serviço
│   └── CatalogoService.cs              # Implementação com dados em memória
├── Controllers/
│   └── ProdutosController.cs           # Controller com Actions e [ChildActionOnly]
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                      # Configuração do Razor clássico
│   ├── Shared/_Layout.cshtml
│   └── Produtos/
│       ├── Index.cshtml                # Consome @Html.Action("ResumoCatalogo")
│       ├── Criar.cshtml                # Formulário com HTML Helpers clássicos
│       └── _ResumoCatalogo.cshtml      # Partial renderizada pela Child Action
├── Content/Site.css
└── Properties/AssemblyInfo.cs
```

### Projeto Moderno: `net10/`
```
net10/
├── ViewsAndRazorDemo.csproj            # SDK-style minimalista visando net10.0
├── Program.cs                          # Configuração de Controllers com Views, Razor Pages e DI
├── Models/
│   ├── ProdutoViewModel.cs             # Record ou classe de dados
│   └── ResumoCatalogoViewModel.cs      # Modelo de resumo
├── Services/
│   ├── ICatalogoService.cs             # Interface de catálogo
│   └── CatalogoService.cs              # Serviço registrado no container de DI
├── ViewComponents/
│   └── ResumoCatalogoViewComponent.cs  # View Component assíncrono moderno
├── Controllers/
│   └── ProdutosController.cs           # Controller MVC limpo
├── Views/
│   ├── _ViewImports.cshtml             # Importação das Tag Helpers do ASP.NET Core
│   ├── _ViewStart.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── Components/
│   │       └── ResumoCatalogo/
│   │           └── Default.cshtml      # Template do View Component
│   └── Produtos/
│       ├── Index.cshtml                # Uso da Tag Helper <vc:resumo-catalogo /> e @inject
│       └── Criar.cshtml                # Formulário usando Tag Helpers nativas
├── Pages/
│   └── ProdutosRazorPage/
│       ├── Index.cshtml                # Exemplo de Razor Page moderna com @page
│       └── Index.cshtml.cs             # PageModel com OnGet e OnPost
├── Properties/
│   └── launchSettings.json             # Perfil Kestrel na porta 5700
└── wwwroot/
    └── css/site.css                    # Estilos visuais
```

---

## Como Executar

### Versão Moderna (.NET 10)

Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/07-views-razor-templating/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5700`).

Telas disponíveis para teste:
- `GET /`: Listagem de produtos no modelo MVC moderno, exibindo o card de resumo gerado pelo View Component `<vc:resumo-catalogo />` e dados adicionais obtidos via `@inject`.
- `GET /produtos/criar`: Formulário construído com Tag Helpers sem objetos anônimos.
- `GET /produtosrazorpage`: Demonstração da mesma listagem e formulário construídos utilizando **Razor Pages** (`PageModel`).

---

### Versão Legada (.NET Framework 4.8.1)

1. Abra o arquivo `ViewsAndRazorDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse no navegador:
   - `http://localhost:5701/`: Listagem MVC que aciona a Child Action `@Html.Action("ResumoCatalogo", "Produtos")`.
   - `http://localhost:5701/produtos/criar`: Formulário MVC construído com os HTML Helpers clássicos (`@Html.TextBoxFor`).

Para compilar via terminal (requer ambiente Windows com MSBuild):
```cmd
msbuild concepts\07-views-razor-templating\framework\ViewsAndRazorDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **A legibilidade do código nos formulários:** Compare [Criar.cshtml do Framework](file:///c:/GITHUB/dotnet-modernization-lab/concepts/07-views-razor-templating/framework/Views/Produtos/Criar.cshtml) com [Criar.cshtml do .NET 10](file:///c:/GITHUB/dotnet-modernization-lab/concepts/07-views-razor-templating/net10/Views/Produtos/Criar.cshtml). No .NET 10, o código é HTML legítimo enriquecido com atributos `asp-*`.
2. **A arquitetura de componentes:** No Framework, veja como o `ProdutosController.cs` precisava ter um método marcado com `[ChildActionOnly]` para poder fornecer os dados do componente de resumo. No .NET 10, o `ResumoCatalogoViewComponent.cs` é uma classe separada, assíncrona, que recebe suas dependências diretamente via injeção.
3. **A simplicidade das Razor Pages:** Inspecione a pasta `Pages/ProdutosRazorPage/` no projeto .NET 10 para entender como o modelo de Razor Pages simplifica fluxos de tela e formulários sem a necessidade de controllers intermediários.
