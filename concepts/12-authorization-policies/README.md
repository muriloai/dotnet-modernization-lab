# Lab 12: Autorização e Políticas

> Comparativo prático entre o modelo clássico de autorização no **.NET Framework 4.8.1** (autorização baseada estritamente em papéis estáticos com `[Authorize(Roles = "...")]`, regras procedurais manuais espalhadas nos controllers e atributos customizados sem injeção de dependência) e o modelo moderno do **.NET 10** (autorização baseada em políticas com `AddAuthorization`, requisitos desacoplados com `IAuthorizationRequirement`, handlers reutilizáveis com injeção de dependência nativa e autorização baseada em recursos).

---

## O que é este conceito?

A autorização é o processo que determina se uma identidade previamente autenticada possui as permissões necessárias para executar uma determinada operação ou acessar um recurso específico no sistema.

- No **.NET Framework 4.8.1**:
  - O controle de acesso básico utilizava o atributo `[Authorize]`, que aceitava apenas strings fixas para nomes de papéis (`Roles = "Administrador,Gerente"`) ou usuários (`Users = "usuario1"`).
  - Toda regra que envolvesse dados contextuais (como "pertencer ao departamento Financeiro", "possuir mais de 18 anos", "ter nível sênior" ou "editar apenas o próprio relatório") exigia a escrita de blocos `if` manuais dentro de cada método de controller ou a criação de classes de atributos herdando de `AuthorizeAttribute`.
  - Atributos customizados no .NET Framework não possuíam ciclo de vida gerenciado pelo contêiner de injeção de dependência. Isso impedia a injeção limpa de repositórios, serviços de auditoria ou configurações nos validadores de acesso.
  - Havia uma explosão de atributos customizados para tentar cobrir combinações de regras de negócio, gerando duplicação de código e acoplamento.
- No **.NET 10**:
  - O ASP.NET Core introduziu o conceito de **Policy-Based Authorization** (Autorização Baseada em Políticas), que desacopla a regra de negócio do ponto onde ela é aplicada.
  - Uma **Política** (`AuthorizationPolicy`) é configurada no arquivo de inicialização (`builder.Services.AddAuthorization(...)`) e pode exigir uma combinação de papéis, declarações (`Claims`), expressões customizadas ou múltiplos **Requisitos** (`IAuthorizationRequirement`).
  - Cada requisito possui um ou mais **Handlers** (`AuthorizationHandler<TRequirement>`), que são registrados no contêiner de DI e recebem normalmente quaisquer serviços necessários via construtor (banco de dados, clientes HTTP, provedores de telemetria).
  - É possível realizar **Autorização Baseada em Recursos** (`IAuthorizationService.AuthorizeAsync`), permitindo avaliar em tempo de execução se o usuário autenticado pode atuar sobre uma entidade específica carregada do banco de dados (exemplo: verificar se o autor do relatório é o usuário atual antes de permitir a alteração).

---

## Por que mudou?

1. **Separação entre Regra e Execução:** No modelo legado, para mudar uma regra de autorização era necessário caçar atributos espalhados por dezenas de controllers ou alterar classes derivadas de `AuthorizeAttribute`. No .NET 10, altera-se apenas a definição da política no registro de serviços.
2. **Fim dos Atributos Monolíticos sem Injeção:** Em versões legadas, validar se um usuário possuía uma permissão dinâmica no banco exigia acessar `ServiceLocator` ou instanciar contextos estáticos. No .NET 10, o `AuthorizationHandler` é um serviço de primeira linha gerenciado pelo contêiner de injeção de dependência.
3. **Composição e Reutilização:** Múltiplas regras independentes (como validação de idade mínima, checagem de departamento e avaliação de senioridade) podem ser combinadas para compor políticas mais complexas sem duplicar linhas de código.
4. **Autorização Granular de Recursos:** Validar propriedade de dados (como garantir que um analista só edite relatórios criados por ele mesmo) no legado era feito com validações manuais ad-hoc dentro de actions. No .NET moderno, utiliza-se a sobrecarga `AuthorizeAsync(User, recurso, politica)`.
5. **Aderência a Minimal APIs e Controllers:** As políticas registradas funcionam com a mesma sintaxe tanto em controllers tradicionais (`[Authorize(Policy = "...")]`) quanto em Minimal APIs (`.RequireAuthorization("...")`).

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Modelo Principal** | Papéis estáticos via `[Authorize(Roles = "...")]` | Políticas desacopladas via `[Authorize(Policy = "...")]` |
| **Definição de Regras** | Strings codificadas diretamente nos atributos dos controllers | Centralizada no `Program.cs` com `builder.Services.AddAuthorization()` |
| **Injeção de Dependências** | Inexistente em atributos (exigia Service Locator manual) | Nativa no construtor de classes `AuthorizationHandler<T>` |
| **Requisitos Customizados** | Classes customizadas herdando de `AuthorizeAttribute` | Interface `IAuthorizationRequirement` desacoplada do handler |
| **Múltiplos Avaliadores** | Difícil coordenação (exigia filtros encadeados) | Suporte nativo a múltiplos handlers para o mesmo requisito |
| **Autorização de Instâncias (Recurso)** | Manual via verificações `if` dentro das actions | Serviço nativo `IAuthorizationService.AuthorizeAsync(User, recurso, ...)` |
| **Testabilidade** | Difícil isolamento devido ao acoplamento com `HttpContextBase` | Handlers testáveis com testes unitários puros via `AuthorizationHandlerContext` |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/` (Porta 6201)

```
framework/
├── AuthorizationPoliciesDemo.csproj   # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                    # Dependências: MVC 5, Web API 2 e Newtonsoft.Json
├── Web.config                         # Configurações do runtime e FormsAuthentication
├── Global.asax                        # Inicialização da aplicação
├── Global.asax.cs                     # Montagem do GenericPrincipal com papéis no PostAuthenticate
├── App_Start/
│   ├── RouteConfig.cs                 # Rotas do ASP.NET MVC
│   └── WebApiConfig.cs                # Rotas da Web API 2
├── Filters/
│   ├── DepartamentoAuthorizeAttribute.cs # Atributo legado customizado herdando AuthorizeAttribute
│   └── IdadeMinimaAuthorizeAttribute.cs  # Atributo legado com parse manual de dados do ticket
├── Models/
│   ├── UsuarioSessao.cs               # Representação do usuário e seus atributos
│   └── RelatorioFinanceiro.cs         # Modelo de relatório para teste de controle de acesso
├── Controllers/
│   ├── ContaController.cs             # Simulação de login com diferentes perfis e dados em UserData
│   ├── DocumentosController.cs        # Controller MVC com [Authorize(Roles)] e filtros legados
│   └── Api/
│       └── DocumentosApiController.cs # Web API 2 demonstrando autorização clássica
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                     # Configuração do Razor 3
│   ├── Shared/
│   │   └── _Layout.cshtml             # Layout base com alternador de perfis
│   ├── Conta/
│   │   └── SelecionarPerfil.cshtml    # Tela para trocar de usuário ativo
│   └── Documentos/
│       └── Index.cshtml               # Painel de teste dos endpoints protegidos por papel e filtro
├── Content/
│   └── Site.css                       # Folha de estilos
└── Properties/
    └── AssemblyInfo.cs
```

### Projeto Moderno: `net10/` (Porta 6200)

```
net10/
├── AuthorizationPoliciesDemo.csproj   # Projeto SDK-style visando net10.0
├── Program.cs                         # Configuração de AddAuthorization, Policies e Handlers
├── Properties/
│   └── launchSettings.json            # Configuração de porta (6200) e perfil Kestrel
├── Authorization/
│   ├── Requirements/
│   │   ├── IdadeMinimaRequirement.cs          # Requisito de idade mínima
│   │   ├── ExperienciaMinimaRequirement.cs    # Requisito de anos de experiência
│   │   └── DonoDoRelatorioRequirement.cs      # Requisito para autorização de recurso
│   └── Handlers/
│       ├── IdadeMinimaHandler.cs              # Handler com leitura de claims tipadas
│       ├── ExperienciaMinimaHandler.cs        # Handler com avaliação de senioridade
│       └── DonoDoRelatorioHandler.cs          # Handler de recurso operando sobre o modelo
├── Models/
│   ├── RelatorioFinanceiro.cs         # Entidade de relatório financeiro
│   └── UsuarioInfo.cs                 # DTO com os dados do usuário autenticado
├── Services/
│   ├── IRelatorioService.cs           # Contrato do repositório/serviço de relatórios
│   └── RelatorioService.cs            # Implementação em memória para testes interativos
├── Controllers/
│   ├── SessaoController.cs            # Alternância de usuários simulados com emissão de Cookie
│   └── DocumentosController.cs        # Endpoints protegidos pelas políticas declarativas
└── wwwroot/
    ├── css/
    │   └── site.css                   # Folha de estilos moderna
    └── index.html                     # Painel visual para alternar usuários e disparar políticas
```

---

## Comparativo de Código: Como era feito vs Como é feito hoje

### 1. Aplicação nos Controllers: Papéis Estáticos vs Políticas Declarativas

#### No .NET Framework 4.8.1 (Abordagem Legada)
```csharp
// DocumentosController.cs
// 1. Apenas papéis estáticos:
[Authorize(Roles = "Administrador,Gerente")]
public ActionResult RelatorioExecutivo()
{
    return View();
}

// 2. Para regras com departamento e idade mínima, exigia filtros customizados acoplados:
[DepartamentoAuthorize("Financeiro")]
[IdadeMinimaAuthorize(18)]
public ActionResult AprovacaoContratos()
{
    // 3. E para regras complexas ou baseadas em instâncias de dados, código procedural manual:
    var relatorio = BuscarRelatorio(id);
    if (!User.IsInRole("Administrador") && relatorio.AutorEmail != User.Identity.Name)
    {
        return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Sem permissão para editar este relatório.");
    }

    return View(relatorio);
}
```

#### No .NET 10 (Abordagem Moderna)
```csharp
// DocumentosController.cs
// 1. Políticas desacopladas e compostas declaradas diretamente no atributo:
[Authorize(Policy = "ApenasFinanceiro")]
[HttpGet("financeiro")]
public IActionResult RelatorioFinanceiro() => Ok(...);

[Authorize(Policy = "AprovadorSenior")]
[HttpGet("aprovacoes-vultosas")]
public IActionResult AprovacaoContratos() => Ok(...);

// 2. Autorização baseada em recursos executada de forma imperativa:
[HttpPut("relatorios/{id:int}")]
public async Task<IActionResult> EditarRelatorio(
    int id,
    [FromServices] IAuthorizationService authService,
    [FromServices] IRelatorioService relatorioService)
{
    var relatorio = await relatorioService.ObterPorIdAsync(id);
    if (relatorio is null) return NotFound();

    // Avalia o handler DonoDoRelatorioHandler sobre a instância do relatório
    var authResult = await authService.AuthorizeAsync(User, relatorio, "DonoOuAdministrador");
    if (!authResult.Succeeded)
    {
        return Forbid();
    }

    return Ok(new { Mensagem = "Relatório atualizado com sucesso!" });
}
```

---

### 2. Implementação de Requisitos e Handlers no .NET 10

A lógica de verificação fica isolada em classes de requisitos e handlers com injeção de dependências:

```csharp
// 1. Requisito (dados da regra)
public sealed class IdadeMinimaRequirement : IAuthorizationRequirement
{
    public int IdadeMinima { get; }
    public IdadeMinimaRequirement(int idadeMinima) => IdadeMinima = idadeMinima;
}

// 2. Handler (lógica de avaliação, com suporte total a injeção de dependências no construtor)
public sealed class IdadeMinimaHandler : AuthorizationHandler<IdadeMinimaRequirement>
{
    private readonly ILogger<IdadeMinimaHandler> _logger;

    public IdadeMinimaHandler(ILogger<IdadeMinimaHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IdadeMinimaRequirement requirement)
    {
        var idadeClaim = context.User.FindFirst("idade");
        if (idadeClaim is not null && int.TryParse(idadeClaim.Value, out var idade))
        {
            if (idade >= requirement.IdadeMinima)
            {
                // Sinaliza que o requisito foi satisfeito com sucesso
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Usuário com {Idade} anos não atingiu o requisito de {Minimo} anos.",
                    idade, requirement.IdadeMinima);
            }
        }

        return Task.CompletedTask;
    }
}
```

E no `Program.cs`, o registro é simples e declarativo:

```csharp
builder.Services.AddSingleton<IAuthorizationHandler, IdadeMinimaHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MaioridadeLegal", policy =>
        policy.Requirements.Add(new IdadeMinimaRequirement(18)));

    options.AddPolicy("AprovadorSenior", policy =>
        policy.RequireRole("Administrador", "Gerente")
              .RequireClaim("departamento", "Financeiro")
              .Requirements.Add(new ExperienciaMinimaRequirement(5)));
});
```

---

## Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/12-authorization-policies/net10
   ```
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```
   http://localhost:6200
   ```
   No painel você poderá:
   - Alternar entre 4 perfis pré-configurados (Admin, Gerente Financeiro Sênior, Analista Financeiro Pleno, Estagiário de TI menor de idade).
   - Testar o disparo das políticas declarativas (`ApenasFinanceiro`, `MaioridadeLegal`, `AprovadorSenior`).
   - Testar a autorização de recurso alterando relatórios próprios versus relatórios de terceiros.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra a pasta `concepts/12-authorization-policies/framework/` no Visual Studio 2022.
2. Defina `AuthorizationPoliciesDemo.csproj` como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e iniciar no IIS Express na porta **6201**.
4. Acesse:
   ```
   http://localhost:6201/
   ```
   Alterne o perfil simulado e observe como o `Authorize(Roles)` e os filtros legados realizam o bloqueio de acessos.
