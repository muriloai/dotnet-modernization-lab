# Lab 10: Validação de Dados e ProblemDetails

> Comparativo prático entre o modelo tradicional de validação no **.NET Framework 4.8.1** (validação manual repetitiva com `if (!ModelState.IsValid)` em cada action, respostas de erro em formato proprietário da Web API 2 e falta de padronização HTTP para falhas de cliente) e os recursos modernos do **.NET 10** (validação automática de modelo com o atributo `[ApiController]`, padronização universal RFC 7807 / RFC 9457 via `ProblemDetails`, validação entre propriedades com `IValidatableObject` e registro global de serviços de problema com `AddProblemDetails`).

---

## O que é este conceito?

A validação de dados de entrada é a primeira linha de defesa de qualquer aplicação web. Ela garante que apenas dados consistentes, íntegros e no formato esperado alcancem as camadas de negócio e persistência.

- No **.NET Framework 4.8.1**:
  - A validação era baseada em atributos do namespace `System.ComponentModel.DataAnnotations` (como `[Required]`, `[StringLength]`, `[Range]`).
  - No ASP.NET Web API 2 e no ASP.NET MVC 5, o framework preenchia o dicionário `ModelState`, mas **não impedia a execução da action**. O desenvolvedor era obrigado a escrever verificações manuais repetitivas (`if (!ModelState.IsValid)`) no início de cada método de controller.
  - Se a validação falhasse na Web API, a chamada a `BadRequest(ModelState)` produzia um objeto JSON com formato proprietário da Microsoft (chaves `Message` e `ModelState`), com prefixos de parâmetro (como `dto.Email`) e sem referências padronizadas da especificação HTTP.
  - Caso o corpo da requisição estivesse corrompido ou ausente, a variável do modelo vinha como `null`, exigindo checagens manuais adicionais (`if (dto == null)`).
  - Cada equipe ou projeto costumava criar classes próprias de resposta (como `ApiResponse<T>` ou filtros customizados `ValidateModelAttribute`) para tentar contornar essa falta de padronização.
- No **.NET 10**:
  - O atributo `[ApiController]` ativa por padrão o filtro de comportamento de API (`ModelStateInvalidFilter`). Se qualquer regra de validação for violada, a requisição é interceptada automaticamente antes que a action seja invocada.
  - A resposta de erro gerada segue estritamente a especificação **RFC 7807** (e sua evolução **RFC 9457**), retornando o Content-Type `application/problem+json` com um objeto padronizado contendo `type`, `title`, `status`, `errors` e `traceId`.
  - O método `builder.Services.AddProblemDetails()` centraliza o tratamento de problemas HTTP em toda a aplicação, garantindo que exceções e códigos de erro de status (como 404, 409 ou 500) também sigam a mesma estrutura sem a necessidade de classes de envelope customizadas.
  - A validação entre campos (cross-property) é implementada de forma declarativa e limpa através de `IValidatableObject` ou validações personalizadas.

---

## Por que mudou?

1. **Eliminação de Código Boilerplate:** No modelo legado, esquecer de incluir `if (!ModelState.IsValid)` em uma action resultava no processamento de dados inválidos pela aplicação. No .NET 10, o `[ApiController]` bloqueia requisições inválidas automaticamente.
2. **Padronização da Indústria (RFC 7807 / RFC 9457):** Antes do padrão ProblemDetails, cada API retornava erros em formatos distintos (algumas em arrays, outras em strings simples, outras em dicionários aninhados). O ProblemDetails definiu um contrato universal compreendido por clientes HTTP, gateways e ferramentas de observabilidade em qualquer linguagem.
3. **Rastreabilidade com `traceId`:** O ProblemDetails moderno inclui nativamente o identificador de rastreamento (`traceId`), permitindo correlacionar um erro retornado ao cliente com os logs internos da aplicação e sistemas de telemetria distribuída.
4. **Respostas Claras para JSON Inválido:** No ASP.NET clássico, um erro de desserialização (como enviar texto em campo numérico) muitas vezes gerava um `null` silencioso ou uma resposta genérica confusa. No .NET 10, o desserializador baseado em `System.Text.Json` preenche o `ProblemDetails` apontando a linha e o caractere exatos da falha sintática.
5. **Integração Unificada com o Pipeline:** Com `AddProblemDetails()`, tanto falhas de validação de modelo quanto regras de negócio violadas (como conflitos de chave ou dados não encontrados) utilizam o mesmo modelo de resposta HTTP.

---

## Comparativo Direto

| Recurso / Dimensão | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Interceptação da Validação** | Manual (`if (!ModelState.IsValid)` obrigatório em cada action) | Automática via atributo `[ApiController]` antes de executar a action |
| **Formato da Resposta de Erro** | Proprietário do ASP.NET Web API (`Message`, `ModelState`) | Padrão internacional RFC 7807 / RFC 9457 (`ProblemDetails`) |
| **Content-Type da Resposta** | `application/json` padrão | `application/problem+json` conforme especificação |
| **Tratamento de Payload Nulo** | Manual (`if (dto == null) return BadRequest(...)`) | Automático pelo pipeline de model binding |
| **Rastreabilidade Integrada** | Ausente (necessário configurar cabeçalhos manualmente) | Nativa via campo `traceId` correlacionado com `HttpContext.TraceIdentifier` |
| **Validação entre Campos** | `IValidatableObject` manual com checagem em `ModelState` | `IValidatableObject` integrado ao ProblemDetails automático |
| **Serviço Global de Problemas** | Inexistente (exigia filtros de exceção manuais) | Nativo via `builder.Services.AddProblemDetails()` |
| **Suporte a Erros de Negócio** | Criação de classes próprias de envelope (ex: `ApiResponse<T>`) | Métodos nativos `TypedResults.Problem()` ou `Results.ValidationProblem()` |

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/` (Porta 6001)

```
framework/
├── DataValidationDemo.csproj          # Projeto clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                    # Dependências: Web API 2, MVC 5 e Newtonsoft.Json
├── Web.config                         # Configurações do runtime e assembly redirects
├── Global.asax                        # Ponto de entrada da aplicação ASP.NET
├── Global.asax.cs                     # Inicialização de rotas MVC e Web API
├── App_Start/
│   ├── RouteConfig.cs                 # Rotas do ASP.NET MVC
│   └── WebApiConfig.cs                # Rotas e formatação do ASP.NET Web API 2
├── Models/
│   ├── ClienteDto.cs                  # DTO para a Web API com DataAnnotations e IValidatableObject
│   ├── ClienteViewModel.cs            # ViewModel para formulários MVC
│   └── Validations/
│       └── CpfValidationAttribute.cs  # Validador customizado de CPF
├── Controllers/
│   ├── Api/
│   │   └── ClientesApiController.cs   # API REST com checagens manuais de ModelState.IsValid
│   └── ClientesMvcController.cs       # Controller MVC com renderização de formulário e erros
├── Views/
│   ├── _ViewStart.cshtml
│   ├── Web.config                     # Configuração do Razor 3
│   ├── Shared/
│   │   └── _Layout.cshtml             # Layout base da aplicação legada
│   └── ClientesMvc/
│       ├── Index.cshtml               # Painel interativo de testes de requisição da API
│       └── Criar.cshtml               # Formulário clássico MVC com Html.ValidationMessageFor
├── Content/
│   └── Site.css                       # Folha de estilos
└── Properties/
    └── AssemblyInfo.cs
```

### Projeto Moderno: `net10/` (Porta 6000)

```
net10/
├── DataValidationDemo.csproj          # Projeto SDK-style visando net10.0
├── Program.cs                         # Configuração de AddControllers, AddProblemDetails e pipeline
├── Properties/
│   └── launchSettings.json            # Configuração de porta (6000) e perfil Kestrel
├── Models/
│   ├── ClienteRequest.cs              # Request DTO com DataAnnotations e IValidatableObject
│   ├── ClienteResponse.cs             # Response DTO de sucesso
│   └── Validations/
│       └── CpfValidationAttribute.cs  # Validador customizado de CPF
├── Controllers/
│   └── ClientesController.cs          # Controller com [ApiController] e retorno de ProblemDetails
└── wwwroot/
    ├── css/
    │   └── site.css                   # Folha de estilos do painel moderno
    └── index.html                     # Painel visual interativo para teste de validações
```

---

## Comparativo de Código: Como era feito vs Como é feito hoje

### 1. Validação na Controller: Manual vs Automática

#### No .NET Framework 4.8.1 (ASP.NET Web API 2)
```csharp
// ClientesApiController.cs
[RoutePrefix("api/clientes")]
public class ClientesApiController : ApiController
{
    [HttpPost]
    [Route("")]
    public IHttpActionResult Criar([FromBody] ClienteDto dto)
    {
        // 1. Verificação manual obrigatória de payload nulo
        if (dto == null)
        {
            return BadRequest("Os dados da requisição não podem ser nulos.");
        }

        // 2. Verificação manual obrigatória de ModelState
        // Se o desenvolvedor esquecer este bloco, o dado inválido é gravado!
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3. Processamento de negócio
        return Ok(new { Mensagem = "Cliente criado com sucesso!", Id = 101 });
    }
}
```

#### No .NET 10 (ASP.NET Core)
```csharp
// ClientesController.cs
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    [HttpPost]
    public ActionResult<ClienteResponse> Criar([FromBody] ClienteRequest request)
    {
        // O método só é executado se os dados forem válidos!
        // O [ApiController] intercepta ModelState inválido e devolve 400 Bad Request
        // com o objeto RFC 7807 HttpValidationProblemDetails automaticamente.

        // Demonstração de ProblemDetails para regra de negócio (ex: duplicidade):
        if (request.Email.EndsWith("@bloqueado.com", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                detail: "O domínio de e-mail informado está bloqueado para novas contas.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito de Regra de Negócio"
            );
        }

        var response = new ClienteResponse(
            Id: 101,
            Nome: request.Nome,
            Email: request.Email,
            Cpf: request.Cpf,
            LimiteCreditoAprovado: request.LimiteCreditoSolicitado
        );

        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }
}
```

---

### 2. Comparativo do JSON de Erro Retornado

Quando uma requisição com e-mail inválido e CPF ausente é enviada para os dois endpoints:

#### Resposta do .NET Framework 4.8.1 (Status 400 Bad Request)
```json
{
  "Message": "The request is invalid.",
  "ModelState": {
    "dto.Email": [
      "O e-mail informado não possui um formato válido."
    ],
    "dto.Cpf": [
      "O CPF é obrigatório."
    ]
  }
}
```
*Observe a chave genérica `Message`, o prefixo do nome do parâmetro `dto.`, a ausência de status no corpo e a falta de padronização RFC.*

#### Resposta do .NET 10 (Status 400 Bad Request / Content-Type: `application/problem+json`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "O e-mail informado não possui um formato válido."
    ],
    "Cpf": [
      "O CPF é obrigatório."
    ]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```
*Formato estritamente aderente à RFC 7807 e RFC 9457, com título internacional, código de status numérico, chaves limpas das propriedades e o `traceId` da requisição para rastreabilidade de suporte.*

---

### 3. Validação entre Múltiplas Propriedades com `IValidatableObject`

Tanto no modelo clássico quanto no moderno é possível implementar a interface `IValidatableObject` para validações compostas. No .NET 10, os erros gerados são incorporados diretamente ao dicionário `errors` do ProblemDetails sem qualquer código adicional.

```csharp
public class ClienteRequest : IValidatableObject
{
    // ... propriedades principais ...

    public bool PossuiRepresentante { get; set; }
    public string? NomeRepresentante { get; set; }
    public string? CpfRepresentante { get; set; }

    public decimal RendaMensal { get; set; }
    public decimal LimiteCreditoSolicitado { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Regra 1: Campos condicionais
        if (PossuiRepresentante)
        {
            if (string.IsNullOrWhiteSpace(NomeRepresentante))
            {
                yield return new ValidationResult(
                    "O nome do representante é obrigatório quando a opção possui representante estiver marcada.",
                    new[] { nameof(NomeRepresentante) }
                );
            }

            if (string.IsNullOrWhiteSpace(CpfRepresentante))
            {
                yield return new ValidationResult(
                    "O CPF do representante é obrigatório quando a opção possui representante estiver marcada.",
                    new[] { nameof(CpfRepresentante) }
                );
            }
        }

        // Regra 2: Cruzamento de valores
        if (LimiteCreditoSolicitado > (RendaMensal * 5))
        {
            yield return new ValidationResult(
                "O limite de crédito solicitado não pode exceder 5 vezes a renda mensal.",
                new[] { nameof(LimiteCreditoSolicitado) }
            );
        }
    }
}
```

---

## Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)

1. Abra o terminal na pasta do projeto:
   ```bash
   cd concepts/10-data-validation/net10
   ```
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```
   http://localhost:6000
   ```
   No painel você poderá disparar payloads válidos, payloads com falha de DataAnnotations, payloads com erro de regra composta e payloads com conflito de negócio para inspecionar os objetos `ProblemDetails` em tempo real.

### Executando o Projeto Legado (.NET Framework 4.8.1)

1. Abra a pasta `concepts/10-data-validation/framework/` no Visual Studio 2022.
2. Defina o projeto `DataValidationDemo.csproj` como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e iniciar o servidor IIS Express na porta **6001**.
4. Acesse:
   ```
   http://localhost:6001/
   ```
   A página inicial disponibiliza tanto o formulário tradicional MVC quanto um console de testes para a Web API 2.
