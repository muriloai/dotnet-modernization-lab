# Lab 13: Práticas de Segurança

> Comparativo prático de defesas contra vulnerabilidades comuns (OWASP Top 10) entre o modelo do **.NET Framework 4.8.1** (anti-forgery manual por action, cabeçalhos CORS genéricos via XML do Web.config, segurança de cabeçalhos estática no IIS e riscos críticos de RCE com o serializador legado `BinaryFormatter`) e as defesas modernas do **.NET 10** (middleware nativo de `UseAntiforgery`, políticas tipadas e restritivas com `AddCors`, middleware programável de cabeçalhos de segurança, HSTS nativo e substituição definitiva do `BinaryFormatter` pelo `System.Text.Json` seguro).

---

## O que é este conceito?

A segurança em aplicações web vai muito além de autenticação e autorização. Ela envolve a aplicação consistente de defesas em camadas contra ameaças do mundo real, tais como Cross-Site Request Forgery (CSRF), vazamento de dados entre origens (CORS mal configurado), ataques de Clickjacking ou MIME-sniffing, e vulnerabilidades críticas de execução remota de código (RCE) causadas por serializadores inseguros.

- No **.NET Framework 4.8.1**:
  - **Proteção Anti-CSRF:** O desenvolvedor precisava lembrar de incluir a tag `@Html.AntiForgeryToken()` em cada formulário Razor e o atributo `[ValidateAntiForgeryToken]` em cada action `POST`. Para requisições AJAX e Web API, o suporte era fragmentado, exigindo a criação manual de filtros customizados de cabeçalho.
  - **Configuração de CORS:** Frequentemente resolvida de forma insegura pela inclusão de tags estáticas `<customHeaders>` no arquivo `Web.config` com `Access-Control-Allow-Origin: *`. Alternativamente, usava-se o pacote `Microsoft.AspNet.WebApi.Cors`, que protegia apenas a Web API 2, deixando rotas de MVC e arquivos estáticos desprotegidos.
  - **Cabeçalhos de Segurança HTTP:** A adição de cabeçalhos defensivos (`X-Frame-Options`, `X-Content-Type-Options`, `Content-Security-Policy`) dependia de configurações manuais no XML do IIS ou da criação de módulos HTTP legados (`IHttpModule`).
  - **Serialização e o Perigo do `BinaryFormatter`:** O `BinaryFormatter` era amplamente utilizado para persistir estados de sessão, caches em memória e transferências de objetos. Ele permitia a desserialização arbitrária de grafos de objetos complexos, o que possibilitava ataques de Remote Code Execution (RCE) a partir de payloads adulterados.
- No **.NET 10**:
  - **Antiforgery Unificado:** O serviço `IAntiforgery` e o middleware `app.UseAntiforgery()` centralizam a proteção contra CSRF. A validação é ativada de forma declarativa e automática, com suporte nativo a cabeçalhos de verificação padronizados (`RequestVerificationToken`) para chamadas assíncronas via `fetch` ou AJAX.
  - **Políticas Restritivas de CORS:** Registradas de forma tipada em C# via `builder.Services.AddCors(...)` e ativadas pelo middleware `app.UseCors(...)`. Permitem isolar domínios autorizados, métodos permitidos e controle estrito de credenciais (`AllowCredentials`).
  - **Segurança de Cabeçalhos via Middleware e HSTS:** O ASP.NET Core oferece middlewares dedicados para HSTS (`app.UseHsts()`) e permite injetar cabeçalhos de segurança defensivos (`CSP`, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`) de maneira programática e parametrizável por ambiente.
  - **Banimento Definitivo do `BinaryFormatter`:** A Microsoft removeu e bloqueou por padrão o `BinaryFormatter` para eliminar vetores de ataque RCE. Toda serialização estruturada e segura é baseada no `System.Text.Json`, que opera apenas com esquemas de dados estritos sem instanciação dinâmica de tipos perigosos.

---

## Por que mudou?

1. **Eliminação de Brechas Críticas de RCE:** Vulnerabilidades históricas de desserialização insegura exploravam o `BinaryFormatter` para comprometer servidores inteiros. O banimento do `BinaryFormatter` no .NET moderno protege a aplicação contra toda essa classe de ataques.
2. **Prevenção Centralizada contra CSRF:** Esquecer um `[ValidateAntiForgeryToken]` em uma action legada abria uma brecha imediata para falsificação de requisições. No .NET moderno, o middleware `UseAntiforgery` atua diretamente no pipeline HTTP.
3. **Fim do CORS Inseguro no XML:** No IIS legado, colocar `<add name="Access-Control-Allow-Origin" value="*" />` no `Web.config` expunha todas as rotas indistintamente. No .NET 10, o CORS é governado por políticas em código integradas ao Endpoint Routing.
4. **Governança de Cabeçalhos HTTP:** A especificação de navegadores modernos exige cabeçalhos defensivos como `Content-Security-Policy` e `Strict-Transport-Security`. Controlar esses cabeçalhos via pipeline em C# permite ajustar regras dinamicamente entre ambientes de desenvolvimento e produção.
5. **Padronização para APIs e Clientes SPA:** O Antiforgery moderno trabalha em harmonia com chamadas REST, expondo tokens leves via cookies e lendo cabeçalhos de requisição de forma transparente.

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Proteção Anti-CSRF** | Manual via `@Html.AntiForgeryToken()` e `[ValidateAntiForgeryToken]` | Unificada via `IAntiforgery` e middleware `app.UseAntiforgery()` |
| **Tokens Anti-CSRF em AJAX** | Sem padrão nativo (exigia filtros manuais em Web API) | Padronizado via cabeçalho `RequestVerificationToken` |
| **Configuração de CORS** | XML `Web.config` frágil ou pacote parcial `WebApi.Cors` | Declarativo e tipado em C# via `AddCors` e `UseCors` |
| **Controle de Origens e Métodos** | Geralmente liberado em massa via `*` no IIS | Políticas nomeadas e restritas por domínio e método |
| **Cabeçalhos de Segurança (CSP, HSTS)** | Configurados estaticamente no XML do IIS ou via `IHttpModule` | Middlewares leves em código (`UseHsts` e middleware de headers) |
| **Serializador de Objetos** | `BinaryFormatter` vulnerável a Remote Code Execution (RCE) | `BinaryFormatter` desabilitado e bloqueado; `System.Text.Json` seguro |
| **Proteção contra Clickjacking** | Header estático no XML `Web.config` | Cabeçalho `X-Frame-Options: DENY` ou CSP via middleware |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/` (Porta 6301)

```
framework/
├── SecurityBestPracticesDemo.csproj   # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                    # Dependências: MVC 5, Web API 2 e Newtonsoft.Json
├── Web.config                         # Cabeçalhos estáticos em customHeaders e falhas de CORS
├── Global.asax                        # Ciclo de vida da aplicação
├── Global.asax.cs                     # Inicialização de rotas
├── App_Start/
│   ├── RouteConfig.cs                 # Rotas do ASP.NET MVC
│   └── WebApiConfig.cs                # Rotas da Web API 2
├── Filters/
│   └── ValidarAntiCsrfWebApiAttribute.cs # Filtro manual para validar token CSRF em Web API
├── Models/
│   ├── TransferenciaFinanceiraModel.cs   # Modelo para demonstrar transações seguras
│   └── ObjetoSessaoLegado.cs             # Modelo decorado com [Serializable] para BinaryFormatter
├── Services/
│   └── SerializadorLegadoService.cs      # Demonstração didática dos riscos do BinaryFormatter
├── Controllers/
│   ├── SegurancaMvcController.cs         # Formulários com e sem ValidateAntiForgeryToken
│   └── Api/
│       └── OperacoesApiController.cs     # Endpoints de API testando CORS e CSRF
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                     # Configuração do Razor 3
│   ├── Shared/
│   │   └── _Layout.cshtml             # Layout base
│   └── SegurancaMvc/
│       ├── Index.cshtml               # Painel de testes de CSRF, CORS e Serialização
│       └── Transferencia.cshtml       # Formulário com proteção manual de token
├── Content/
│   └── Site.css                       # Folha de estilos
└── Properties/
    └── AssemblyInfo.cs
```

### Projeto Moderno: `net10/` (Porta 6300)

```
net10/
├── SecurityBestPracticesDemo.csproj   # Projeto SDK-style visando net10.0
├── Program.cs                         # Configuração de UseAntiforgery, AddCors, Security Headers e HSTS
├── Properties/
│   └── launchSettings.json            # Configuração de porta (6300) e perfil Kestrel
├── Middlewares/
│   └── SecurityHeadersMiddleware.cs   # Middleware moderno injetando CSP, HSTS e X-Frame-Options
├── Models/
│   ├── TransferenciaRequest.cs        # Modelo para operações financeiras protegidas
│   └── TransacaoRegistro.cs           # Registro imutável de transação
├── Services/
│   ├── ITransferenciaService.cs       # Serviço de operações seguras
│   └── TransferenciaService.cs        # Implementação com serialização segura em System.Text.Json
├── Controllers/
│   ├── SegurancaController.cs         # Endpoints protegidos por UseAntiforgery e políticas de CORS
│   └── AuditoriaController.cs         # Demonstração de auditoria e inspeção de cabeçalhos de segurança
└── wwwroot/
    ├── css/
    │   └── site.css                   # Folha de estilos moderna
    └── index.html                     # Painel visual interativo para testes de CSRF, CORS e Headers
```

---

## Comparativo de Código: Como era feito vs Como é feito hoje

### 1. Proteção Anti-CSRF: Validação Manual vs Middleware Integrado

#### No .NET Framework 4.8.1
```csharp
// SegurancaMvcController.cs
// No MVC tradicional, o desenvolvedor é obrigado a lembrar de colocar o atributo em cada action POST
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Transferir(TransferenciaFinanceiraModel model)
{
    // Se o desenvolvedor esquecer [ValidateAntiForgeryToken], a aplicação fica vulnerável a CSRF!
    ProcessarTransferencia(model);
    return View("Sucesso");
}
```

#### No .NET 10
```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = ".ModernLab.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// O middleware atua no pipeline unificado para rotas MVC e Minimal APIs
app.UseAntiforgery();
```

---

### 2. Configuração de CORS: XML Inseguro vs Políticas Declarativas em Código

#### No .NET Framework 4.8.1 (Web.config)
```xml
<!-- Web.config legado: libera todas as origens sem controle de métodos ou cabeçalhos -->
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <add name="Access-Control-Allow-Origin" value="*" />
      <add name="Access-Control-Allow-Methods" value="GET,POST,PUT,DELETE" />
      <add name="Access-Control-Allow-Headers" value="Content-Type,Authorization" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

#### No .NET 10 (Program.cs)
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaRestritaParceiro", policy =>
    {
        policy.WithOrigins("https://portal-parceiro.empresa.com.br")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization", "X-XSRF-TOKEN")
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("PoliticaRestritaParceiro");
```

---

### 3. Eliminação do Vulnerável `BinaryFormatter`

#### No .NET Framework 4.8.1 (Vulnerabilidade Crítica de RCE)
```csharp
// Objeto que aceita qualquer grafo de classes durante a desserialização
var formatter = new BinaryFormatter();
using (var stream = new MemoryStream(dadosRecebidos))
{
    // Risco crítico: se os bytes forem forjados por um atacante,
    // comandos arbitrários do sistema operacional podem ser executados no servidor!
    var objeto = (ObjetoSessaoLegado)formatter.Deserialize(stream);
}
```

#### No .NET 10 (Serialização Segura com System.Text.Json)
```csharp
// O BinaryFormatter é completamente bloqueado no .NET 10 por segurança.
// A serialização é estrita, tipada e segura com System.Text.Json:
var transacao = JsonSerializer.Deserialize<TransacaoRegistro>(jsonBytes, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    MaxDepth = 8 // Proteção contra estouro de pilha por payloads maliciosos aninhados
});
```

---

## Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/13-security-best-practices/net10
   ```
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```
   http://localhost:6300
   ```
   No painel você poderá:
   - Testar o envio de requisições POST com e sem o token anti-forgery (`X-XSRF-TOKEN`).
   - Simular requisições de origens permitidas vs origens bloqueadas pelo CORS.
   - Inspecionar todos os cabeçalhos de segurança HTTP injetados dinamicamente pelo middleware (`Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options`).
   - Testar a serialização segura com `System.Text.Json` e compreender o bloqueio do `BinaryFormatter`.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra a pasta `concepts/13-security-best-practices/framework/` no Visual Studio 2022.
2. Defina `SecurityBestPracticesDemo.csproj` como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e iniciar no IIS Express na porta **6301**.
4. Acesse:
   ```
   http://localhost:6301/
   ```
   Teste o formulário clássico com e sem token AntiForgery, inspecione as tags de cabeçalho do `Web.config` e analise o código didático do `BinaryFormatter`.
