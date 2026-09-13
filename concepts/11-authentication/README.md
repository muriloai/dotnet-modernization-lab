# Lab 11: Autenticação

> Comparativo prático entre a autenticação clássica no **.NET Framework 4.8.1** (configuração via XML com `FormsAuthentication`, criptografia dependente de `MachineKey` estática no Web.config, modelo legado de `IPrincipal` e `GenericIdentity`, e dificuldade para suportar múltiplos esquemas) e os recursos modernos de segurança do **.NET 10** (middleware nativo com `AddAuthentication`, `ClaimsPrincipal` extensível com claims tipadas, suporte integrado a Cookie e JWT Bearer no mesmo projeto, e gerenciamento de chaves moderno via Data Protection API).

---

## O que é este conceito?

A autenticação é o processo de verificar e assegurar a identidade de um usuário ou serviço que interage com o sistema. Ela precede a autorização e estabelece quem está realizando cada operação.

- No **.NET Framework 4.8.1**:
  - A autenticação para aplicações web era tradicionalmente configurada no arquivo `Web.config` dentro da seção `<authentication mode="Forms">`.
  - A sessão autenticada era mantida por meio de um cookie binário criptografado (frequentemente nomeado `.ASPXAUTH`) gerado por `FormsAuthentication.Encrypt()`.
  - A integridade e o sigilo do ticket dependiam diretamente da chave configurada na seção `<machineKey>` do servidor ou do arquivo de configuração. Em ambientes com múltiplos servidores (Web Farms) ou nuvem, chaves dessincronizadas geravam falhas intermitentes e desconexões constantes.
  - A identidade do usuário era modelada pelas interfaces `IPrincipal` e `IIdentity`, com implementações rígidas como `GenericPrincipal` e `FormsIdentity`.
  - Para persistir informações adicionais (como e-mail, perfil ou departamento), o desenvolvedor precisava codificar e decodificar manualmente uma string única no campo `UserData` do `FormsAuthenticationTicket`.
  - Oferecer suporte conjunto a autenticação por cookie para o navegador e autenticação por tokens (como JWT) para APIs exigia a montagem de manipuladores de mensagem (`DelegatingHandler`) manuais e códigos complexos no `Global.asax.cs`.
- No **.NET 10**:
  - A autenticação é registrada no contêiner de injeção de dependências através de `builder.Services.AddAuthentication(...)` e ativada no pipeline pelo middleware `app.UseAuthentication()`.
  - O ecossistema adota o modelo universal de **Claims-Based Identity**, onde qualquer usuário é representado por um `ClaimsPrincipal` contendo uma coleção de declarações (`Claim`) com tipo, valor e emissor.
  - Múltiplos esquemas de autenticação (`CookieAuthenticationDefaults`, `JwtBearerDefaults`, OAuth, OpenID Connect) podem coexistir pacificamente no mesmo projeto, permitindo que rotas MVC usem cookies enquanto rotas de API usem tokens JWT.
  - O mecanismo de proteção criptográfica utiliza a **Data Protection API** (`Microsoft.AspNetCore.DataProtection`), com gerenciamento automático de ciclo de vida de chaves, rotação programada e suporte nativo a armazenamento compartilhado (Redis, Azure Key Vault, Google Cloud KMS ou discos de rede).
  - Operações de login e logout são assíncronas e explicitamente controladas por `HttpContext.SignInAsync(...)` e `HttpContext.SignOutAsync(...)`.

---

## Por que mudou?

1. **Eliminação da Dependência do MachineKey:** O modelo clássico de `MachineKey` com algoritmos herdados (3DES, AES estático) era difícil de manter em ambientes dinâmicos, contêineres Docker e serviços escaláveis em nuvem. A Data Protection API moderna gera, rotaciona e isola chaves de forma segura e automatizada.
2. **Suporte Híbrido Nativo (Cookie + JWT):** Aplicações modernas frequentemente combinam páginas web renderizadas no servidor com endpoints REST consumidos por clientes móveis ou SPAs. No .NET 10, basta encadear `.AddCookie()` e `.AddJwtBearer()` para atender ambos os cenários na mesma aplicação.
3. **Flexibilidade de Dados com Claims:** Em vez de concatenar dados do usuário em uma string bruta separada por vírgulas ou ponto e vírgula no `UserData` do ticket, as claims armazenam qualquer atributo (e-mail, identificador, papéis, departamento, empresa) de forma estruturada e padronizada.
4. **Independência de Sistema Operacional:** O `FormsAuthentication` estava acoplado ao pipeline do IIS e ao registro do Windows. O sistema de autenticação do ASP.NET Core opera de forma idêntica em Linux, macOS e Windows.
5. **Integração Fluida com Injeção de Dependências:** Serviços de autenticação e provedores de credenciais têm acesso direto a todo o grafo de injeção de dependências do ASP.NET Core, simplificando integrações com bases de dados, caches e provedores externos.

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Definição do Mecanismo** | XML `<system.web>/<authentication mode="Forms">` | Código C# com `builder.Services.AddAuthentication(...)` |
| **Pipeline de Execução** | Módulo HTTP nativo do IIS (`FormsAuthenticationModule`) | Middleware linear `app.UseAuthentication()` no pipeline |
| **Criptografia de Cookies** | Estática via `<machineKey>` do IIS ou `Web.config` | Dinâmica e extensível via ASP.NET Core Data Protection API |
| **Representação da Identidade** | `IPrincipal` e `IIdentity` (ex: `GenericPrincipal`) | `ClaimsPrincipal` e `ClaimsIdentity` baseados em declarações |
| **Armazenamento de Metadados** | String livre no campo `UserData` do ticket | Coleção fortemente tipada de `Claim` |
| **Múltiplos Esquemas Simultâneos** | Difícil configuração via handlers HTTP manuais | Suporte nativo encadeando esquemas (ex: Cookie + JWT) |
| **Emissão de Login** | `FormsAuthentication.SetAuthCookie` ou ticket manual | `await HttpContext.SignInAsync(scheme, principal)` |
| **Encerramento de Sessão** | `FormsAuthentication.SignOut()` síncrono | `await HttpContext.SignOutAsync(scheme)` assíncrono |
| **Portabilidade** | Exclusivo para Windows e IIS | Multiplataforma (Linux, macOS, Windows e Docker) |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/` (Porta 6101)

```
framework/
├── AuthenticationDemo.csproj          # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                    # Dependências: MVC 5, Web API 2 e Newtonsoft.Json
├── Web.config                         # Configuração do modo Forms e máquina
├── Global.asax                        # Ponto de entrada do ciclo de vida
├── Global.asax.cs                     # Tratamento de PostAuthenticateRequest para extrair ticket
├── App_Start/
│   ├── RouteConfig.cs                 # Rotas do ASP.NET MVC
│   └── WebApiConfig.cs                # Rotas da Web API 2
├── Models/
│   ├── LoginViewModel.cs              # Modelo para o formulário de login
│   ├── UsuarioInfo.cs                 # Representação de usuário para simulação
│   └── PerfilUsuario.cs               # Enumerador de perfis (Admin, Operador)
├── Controllers/
│   ├── ContaController.cs             # Ações de login, logout e geração de ticket legado
│   ├── HomeController.cs              # Painel protegido inspecionando HttpContext.Current.User
│   └── Api/
│       └── PerfilApiController.cs     # Endpoint de API demonstrando autenticação clássica
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                     # Configuração do Razor 3
│   ├── Shared/
│   │   └── _Layout.cshtml             # Layout com status de autenticação
│   ├── Conta/
│   │   └── Login.cshtml               # Formulário clássico de login
│   └── Home/
│       └── Index.cshtml               # Painel de inspeção de identidade e testes de API
├── Content/
│   └── Site.css                       # Folha de estilos
└── Properties/
    └── AssemblyInfo.cs
```

### Projeto Moderno: `net10/` (Porta 6100)

```
net10/
├── AuthenticationDemo.csproj          # SDK-style visando net10.0 com JwtBearer
├── Program.cs                         # Configuração de AddAuthentication (Cookie + JWT) e pipeline
├── Properties/
│   └── launchSettings.json            # Configuração de porta (6100) e perfil Kestrel
├── Models/
│   ├── LoginRequest.cs                # DTO para requisição de login
│   ├── UsuarioResponse.cs             # Informações do usuário autenticado
│   └── TokenResponse.cs               # DTO para entrega de token JWT gerado
├── Services/
│   ├── IUsuarioService.cs             # Contrato do serviço de usuários simulados
│   └── UsuarioService.cs              # Implementação com validação de credenciais
├── Controllers/
│   ├── AutenticacaoController.cs      # Login interativo (Cookie), geração de JWT e Logout
│   └── PainelProtegidoController.cs   # Endpoints protegidos demonstrando leitura de Claims
└── wwwroot/
    ├── css/
    │   └── site.css                   # Folha de estilos do painel moderno
    └── index.html                     # Painel visual interativo para teste de Cookie e JWT
```

---

## Comparativo de Código: Como era feito vs Como é feito hoje

### 1. Configuração e Ativação do Sistema de Autenticação

#### No .NET Framework 4.8.1 (Web.config e Global.asax)
```xml
<!-- Web.config -->
<system.web>
  <authentication mode="Forms">
    <forms loginUrl="~/Conta/Login" timeout="30" name=".ASPXAUTH" slidingExpiration="true" />
  </authentication>
</system.web>
```

```csharp
// Global.asax.cs
protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
{
    var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
    if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
    {
        // Decriptografia manual dependente do MachineKey
        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
        if (ticket != null && !ticket.Expired)
        {
            // Os papéis e dados extras precisavam ser desmembrados de uma string
            string[] roles = ticket.UserData.Split(';');
            var identity = new FormsIdentity(ticket);
            var principal = new GenericPrincipal(identity, roles);

            HttpContext.Current.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
        }
    }
}
```

#### No .NET 10 (Program.cs)
```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "DotNetModernizationLab",
            ValidAudience = "DotNetModernizationLab",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("ChaveSeguraDePeloMenos32BytesParaLaboratorio2026!"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middlewares lineares e modulares
app.UseAuthentication();
app.UseAuthorization();
```

---

### 2. Efetuando o Login e Gravando a Identidade

#### No .NET Framework 4.8.1
```csharp
// ContaController.cs
var ticket = new FormsAuthenticationTicket(
    1,                                  // Versão do ticket
    usuario.Email,                      // Nome da identidade
    DateTime.Now,                       // Data de emissão
    DateTime.Now.AddMinutes(30),        // Data de expiração
    lembrarMe,                          // Persistência
    $"{usuario.Perfil};{usuario.Departamento}", // String concatenada de dados extras
    FormsAuthentication.FormsCookiePath
);

string encryptedTicket = FormsAuthentication.Encrypt(ticket);
var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
{
    HttpOnly = true,
    Path = FormsAuthentication.FormsCookiePath,
    Secure = FormsAuthentication.RequireSSL
};

Response.Cookies.Add(cookie);
```

#### No .NET 10
```csharp
// AutenticacaoController.cs
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    new(ClaimTypes.Name, usuario.Nome),
    new(ClaimTypes.Email, usuario.Email),
    new(ClaimTypes.Role, usuario.Perfil),
    new("departamento", usuario.Departamento),
    new("ultimo_acesso", DateTime.UtcNow.ToString("O"))
};

var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
var principal = new ClaimsPrincipal(identity);

await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    principal,
    new AuthenticationProperties
    {
        IsPersistent = request.LembrarMe,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
    }
);
```

---

## Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto moderno:
   ```bash
   cd concepts/11-authentication/net10
   ```
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse a aplicação no navegador:
   ```
   http://localhost:6100
   ```
   No painel interativo, você poderá:
   - Efetuar login com contas de demonstração (`admin@empresa.com` ou `operador@empresa.com`, senha `123456`).
   - Inspecionar todas as Claims decodificadas do `ClaimsPrincipal`.
   - Gerar um token JWT Bearer e disparar requisições autenticadas para endpoints de API com o cabeçalho `Authorization: Bearer <token>`.
   - Efetuar logout e constatar a invalidação da sessão.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra a pasta `concepts/11-authentication/framework/` no Visual Studio 2022.
2. Defina `AuthenticationDemo.csproj` como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e iniciar no IIS Express na porta **6101**.
4. Acesse:
   ```
   http://localhost:6101/
   ```
   Teste o formulário clássico com `FormsAuthentication`, a extração do ticket criptografado e a leitura do `IPrincipal` no controller.
