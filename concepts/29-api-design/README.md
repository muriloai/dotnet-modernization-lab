# Conceito 29: Padrões de Design de API

Este laboratório compara as práticas de design de APIs REST entre o ASP.NET Web API / MVC no .NET Framework 4.8.1 e o ecossistema moderno de APIs no .NET 10.

---

## 1. Cenário e Justificativa

O design de contratos de API determina a longevidade, interoperabilidade e manutenibilidade dos serviços em ambientes distribuídos.

### No .NET Framework 4.8.1 (Legado)
* **Versionamento Manual e Frágil:** O versionamento de endpoints era frequentemente implementado fixando literais nas rotas (`[Route("api/v1/produtos")]` e `[Route("api/v2/produtos")]`), sem suporte uniforme para versionamento via query string (`?api-version=2.0`) ou cabeçalho HTTP (`X-Api-Version`).
* **Envelopes Ad-Hoc:** Sem um padrão oficial para respostas e erros, APIs adotavam classes manuais como `ApiResponse<T>` contendo `{ Sucesso: bool, Dados: T, MensagemErro: string }`. Isso forçava clientes a ignorar códigos de status HTTP semânticos (retornando HTTP 200 até para erros).
* **Ausência de Padronização RFC 7807:** Falhas de validação e erros internos geravam estruturas heterogêneas de JSON ou telas amarelas (HTML) do IIS w3wp quando ocorriam exceções não tratadas.
* **Documentação Swagger Antiga:** O pacote `Swashbuckle 5.x` era limitado ao OpenAPI 2.0 (Swagger 2.0), com geração complexa via Reflection pesada e sem tipagem nativa de metadados de endpoint.

### No .NET 10 (Moderno)
* **Versionamento Declarativo (Asp.Versioning):** Suporte nativo e desacoplado através de `ApiVersion`, permitindo versionamento por segmento de URL (`v{version:apiVersion}`), query string (`api-version=2.0`) ou cabeçalho (`X-Api-Version`) com resolução automática e fallback.
* **RFC 7807 ProblemDetails:** Padrão universal IETF adotado nativamente pelo ASP.NET Core (`ProblemDetails` e `ValidationProblemDetails`), garantindo respostas de erro ricas e padronizadas em toda a organização (`type`, `title`, `status`, `detail`, `instance` e `extensions`).
* **OpenAPI 3.1 Integrado:** Geração nativa de esquemas e documentação em tempo de compilação ou execução via `Microsoft.AspNetCore.OpenApi` com tipagem estrita de metadados (`ProducesResponseType`, `.ProducesProblem()`).
* **HATEOAS e Hipermídia:** Capacidade de enriquecer DTOs de saída com links contextuais (`_links`) para navegação autodescritiva (self, update, delete).

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Versionamento** | Rotas manuais fixas (`api/v1/`) | Declarativo com `Asp.Versioning` (URL, query string ou header) |
| **Formato de Erros** | Envelopes manuais ou HTML 500 | Padrão IETF RFC 7807 (`ProblemDetails`) nativo |
| **Status HTTP Semântico** | Frequente uso de HTTP 200 com flag `{ sucesso: false }` | Códigos HTTP semânticos (200, 201, 204, 400, 404, 409, 422) |
| **Documentação de API** | Swashbuckle 5.x legado (OpenAPI 2.0) | OpenAPI 3.1 nativo do ASP.NET Core |
| **Hipermídia / Links** | Raro ou construído via concatenação de strings | DTOs com dicionário ou lista de links HATEOAS tipados |
| **Negociação de Conteúdo** | Formatters legados do Web API 2 | Formatters eficientes System.Text.Json e XML nativos |

---

## 3. Estrutura dos Projetos

```
concepts/29-api-design/
├── README.md
├── framework/
│   ├── ApiDesignDemo.csproj
│   ├── Global.asax / Global.asax.cs
│   ├── Web.config
│   ├── Models/
│   │   ├── RespostaEnvelopeLegada.cs
│   │   └── ProdutoLegado.cs
│   ├── Controllers/
│   │   ├── ProdutosV1LegadoController.cs
│   │   └── ApiDesignLegadoController.cs
│   └── Views/
│       └── ApiDesignLegado/Index.cshtml
└── net10/
    ├── ApiDesignDemo.csproj
    ├── Program.cs
    ├── Models/
    │   ├── ProdutoDtos.cs
    │   └── LinkHateoas.cs
    ├── Controllers/
    │   ├── v1/ProdutosController.cs
    │   └── v2/ProdutosController.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 7900)
```powershell
cd concepts/29-api-design/net10
dotnet run
```
Acesse no navegador: `http://localhost:7900`

### .NET Framework 4.8.1 (Porta 7901)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\29-api-design\framework /port:7901
```
Acesse no navegador: `http://localhost:7901`
