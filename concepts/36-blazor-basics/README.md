# Conceito 36: Blazor no .NET 10 vs ASP.NET Web Forms no .NET Framework 4.8.1

Este laboratório demonstra a profunda transformação no desenvolvimento de interfaces de usuário web no ecossistema .NET, confrontando o modelo histórico do **ASP.NET Web Forms** no .NET Framework 4.8.1 (baseado em postbacks completos de página e no campo oculto `__VIEWSTATE`) com o paradigma moderno de componentes reativos em C# com **Blazor** no .NET 10.

---

## 1. Visão Geral e Contexto Prático

No início da web corporativa, o ASP.NET Web Forms foi projetado para permitir que desenvolvedores do Visual Basic e Windows Forms construíssem aplicações web usando um modelo de eventos orientado a controles de tela (como `Button_Click` e `GridView`). Para simular o comportamento de uma aplicação desktop em um protocolo HTTP sem estado, a Microsoft introduziu:

1. **O Campo Oculto `__VIEWSTATE`:** Uma string serializada em Base64 colocada em um `<input type="hidden">` contendo as propriedades de todos os controles da página. A cada clique, o ViewState inteiro viajava do navegador ao servidor e do servidor de volta ao navegador.
2. **PostBacks Completos:** Toda ação do usuário (clicar em um botão, mudar um dropdown) submetia o formulário inteiro via HTTP POST, renderizando novamente a página HTML completa no IIS.

Com a evolução da web para Single Page Applications (SPAs) e consumo mobile com limites de banda, esse modelo tornou-se insustentável. O **Blazor** resolve isso executando C# diretamente na camada de interface sem necessidade de ViewState nem de JavaScript complexo.

---

## 2. O Cenário Legado no ASP.NET Web Forms 4.8.1

No ASP.NET Web Forms, o desenvolvimento era caracterizado por:

* **Inchaço de Rede (Payload Bloat):** Páginas com formulários e tabelas corporativas frequentemente geravam ViewStates de 100 KB a mais de 2 MB. O usuário em redes móveis ou com latência sofria travamentos a cada clique.
* **Ciclo de Vida Intricado:** Uma requisição executava uma sequência complexa de estágios no servidor: `Page_Init`, `LoadViewState`, `Page_Load`, `RaisePostBackEvent`, `Page_PreRender`, `SaveViewState` e `Render`. Erros sutis de inicialização ocorriam quando propriedades eram alteradas no momento incorreto do ciclo.
* **Markup Poluído e IDs Incompatíveis:** Controles `<asp:TextBox>` e `<asp:Button>` geravam elementos HTML com IDs distorcidos (como `ctl00$MainContent$btnSalvar`), dificultando a estilização CSS e scripts customizados.
* **Acoplamento Total com o Windows e IIS:** O Web Forms dependia estruturalmente de bibliotecas nativas do `System.Web.dll`, impedindo a execução no Linux, contêineres leves ou nuvens modernas.

---

## 3. A Solução Moderna com Blazor no .NET 10

No .NET 10, o Blazor estabelece um padrão de desenvolvimento web produtivo, performático e moderno:

* **Zero ViewState:** O estado da aplicação permanece em memória no servidor (no modo Interactive Server via conexão WebSocket) ou na memória do navegador (no modo Interactive WebAssembly). Nenhum campo oculto de estado é trafegado pela rede.
* **RenderTree e Diffs Binários:** Quando o estado C# muda (ex: clicar em um botão), o mecanismo de renderização do Blazor calcula a diferença mínima na árvore de componentes (RenderTree Diff) e envia apenas alguns bytes codificados via WebSocket. O navegador altera cirurgicamente apenas o nó de texto ou elemento afetado.
* **Data-Binding Bidirecional Declarativo:** A diretiva `@bind` sincroniza propriedades de modelos C# e entradas HTML em tempo real, sem necessidade de chamar métodos manuais de `DataBind()`.
* **Multiplataforma e Leve:** Executa sobre o runtime rápido Kestrel no Linux, contêineres Docker e ambientes serverless, com suporte a renderização híbrida e estática unificada.

---

## 4. Comparativo de Código

### Contador no Legado (ASP.NET Web Forms com ViewState)

```csharp
// Default.aspx.cs no .NET Framework 4.8.1
public partial class Default : Page
{
    protected void btnIncrementar_Click(object sender, EventArgs e)
    {
        // Estado armazenado no dicionario ViewState serializado em Base64
        int contador = (ViewState["Contador"] != null) ? (int)ViewState["Contador"] : 0;
        contador++;
        ViewState["Contador"] = contador;
        lblContador.Text = contador.ToString();
        // Causa recarregamento completo da pagina (Full Page PostBack)
    }
}
```

### Contador no Moderno (Componente Blazor no .NET 10)

```razor
@* Home.razor no .NET 10 *@
@rendermode InteractiveServer

<div class="counter-display">@currentCount</div>
<button class="blazor-btn" @onclick="IncrementCount">+ Incrementar</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        // Estado natural em C# mantido em memoria; o Blazor atualiza apenas o numero na tela
        currentCount++;
    }
}
```

---

## 5. Tabela Comparativa Estrutural

| Aspecto | ASP.NET Web Forms (.NET Framework 4.8.1) | Blazor (.NET 10) |
| :--- | :--- | :--- |
| **Modelo de Execução** | PostBack com recarregamento completo da página | Renderização de componentes reativos no DOM Virtual |
| **Gerenciamento de Estado** | String Base64 `__VIEWSTATE` trafegada a cada round-trip | Estado em memória nativa C# (Server ou WebAssembly) |
| **Sobrecarga de Rede** | Alta (50 KB a 2 MB por clique do usuário) | Mínima (~100 bytes de diff binário via WebSocket) |
| **Ciclo de Vida** | Mais de 10 estágios acoplados (Page_Load, PreRender...) | Intuitivo (`OnInitializedAsync`, `OnParametersSetAsync`) |
| **Linguagem de Interface** | Controles proprietários `<asp:*>` com IDs distorcidos | HTML5 semântico com diretivas C# limpas (`@bind`, `@onclick`) |
| **Compatibilidade** | Exclusivo de Windows e IIS | Linux, macOS, Windows, contêineres Docker e Kestrel |

---

## 6. Estrutura dos Projetos

```text
concepts/36-blazor-basics/
|-- README.md
|-- framework/
|   |-- Properties/
|   |   `-- AssemblyInfo.cs
|   |-- Default.aspx
|   |-- Default.aspx.cs
|   |-- Default.aspx.designer.cs
|   |-- Global.asax
|   |-- Global.asax.cs
|   |-- Site.Master
|   |-- Site.Master.cs
|   |-- Site.Master.designer.cs
|   |-- Site.css
|   |-- Web.config
|   `-- WebFormsLegadoDemo.csproj
`-- net10/
    |-- Components/
    |   |-- Layout/
    |   |   |-- MainLayout.razor
    |   |   `-- ReconnectModal.razor
    |   |-- Pages/
    |   |   |-- Error.razor
    |   |   |-- Home.razor
    |   |   `-- NotFound.razor
    |   |-- App.razor
    |   |-- Routes.razor
    |   `-- _Imports.razor
    |-- Properties/
    |   `-- launchSettings.json
    |-- wwwroot/
    |   `-- app.css
    |-- BlazorModernoDemo.csproj
    |-- Program.cs
    `-- appsettings.json
```

---

## 7. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10 com Blazor)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/36-blazor-basics/net10
   ```
2. Execute a aplicação:
   ```bash
   dotnet run
   ```
3. Acesse a aplicação no navegador:
   `http://localhost:8600`
4. Experimente a interatividade do Blazor:
   * Clique repetidamente em **+ Incrementar** e observe que o contador muda instantaneamente sem recarregar a aba do navegador.
   * Digite novas tarefas na lista interativa e marque caixas de seleção, notando o data binding bidirecional reativo.
   * Observe o comparador de sobrecarga de rede demonstrando a redução de 99.8% no tráfego de dados.

### Executando o Projeto Legado (.NET Framework 4.8.1 com Web Forms)

1. Abra o arquivo `WebFormsLegadoDemo.csproj` no Visual Studio 2022.
2. Inicie a aplicação com IIS Express na porta configurada:
   `http://localhost:8601`
3. Ao clicar no botão **Incrementar (PostBack)**, observe a tela piscar (reload completo do navegador), o contador de ciclos de vida aumentando e a presença do campo oculto `__VIEWSTATE` no código-fonte da página.
