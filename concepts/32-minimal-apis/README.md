# Conceito 32: Minimal APIs

Este laboratório compara os controladores tradicionais do ASP.NET Web API 2 no .NET Framework 4.8.1 com o padrão Minimal APIs no .NET 10.

---

## 1. Cenário e Justificativa

A evolução de arquiteturas para microsserviços, funções em nuvem (Serverless) e serviços de baixa latência expôs o custo de sobrecarga da infraestrutura tradicional baseada em classes controladoras pesadas.

### No .NET Framework 4.8.1 (Legado)
* **Cerimônia Obrigatória:** Para expor qualquer endpoint HTTP simples, era mandatório criar uma classe herdando de `ApiController` (ou `Controller`), definir construtores para injeção de dependências e configurar rotas globais ou via atributos.
* **Sobrecarga de Instanciação e Reflection:** O Web API 2 utilizava Reflection em tempo de execução para inspecionar assemblies, descobrir tipos de controllers, resolver dependências e instanciar um novo controller para cada requisição HTTP recebida.
* **Dificuldade de Modularização Leve:** Criar uma API com poucas rotas exigia a mesma estrutura de um monólito completo, aumentando o tempo de boot e o consumo de memória.
* **Testes com HttpContext Complexo:** Testar unitariamente uma action exigia criar mocks complexos de `HttpRequestMessage`, `HttpConfiguration` e controladores.

### No .NET 10 (Moderno)
* **Zero Boilerplate:** Rotas e lógicas de negócios podem ser mapeadas diretamente através de lambdas (`app.MapGet`, `app.MapPost`, `app.MapDelete`) ou separadas em métodos estáticos concisos.
* **Route Groups:** Permite agrupar conjuntos de rotas com prefixos, autorização, validação e metadados compartilhados de forma declarativa (`app.MapGroup("/api/tarefas")`).
* **Endpoint Filters:** Interceptação nativa e encadeada com `AddEndpointFilter`, permitindo validação, logging e auditoria sem a complexidade de classes de Action Filter.
* **TypedResults:** Fornece retornos fortemente tipados (`TypedResults.Ok<T>`, `TypedResults.NotFound`, `TypedResults.Created`), permitindo testes unitários diretos sem necessidade de mockar `HttpContext` e gerando metadados de documentação OpenAPI automaticamente.
* **Compilação com Source Generators:** No .NET 10, o mecanismo de roteamento e injeção de parâmetros nas Minimal APIs é pré-computado via geradores de código em tempo de compilação, eliminando o uso de Reflection e entregando o maior throughput da história do ecossistema .NET.

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 (Web API 2) | .NET 10 (Minimal APIs) |
| :--- | :--- | :--- |
| **Definição de Endpoints** | Classes herdando de `ApiController` | Mapeamento direto funcional (`app.MapGet/MapPost`) |
| **Sobrecarga de Boot e Request** | Reflection dinâmica e instanciação por request | Source Generators e lambdas compiladas |
| **Agrupamento de Rotas** | `[RoutePrefix]` em classes separadas | `RouteGroupBuilder` declarativo (`MapGroup`) |
| **Filtros de Interceptação** | Action Filters herdando de `ActionFilterAttribute` | Endpoint Filters funcionais (`AddEndpointFilter`) |
| **Retorno de Ações** | `IHttpActionResult` ou `HttpResponseMessage` | `TypedResults` fortemente tipado e testável |
| **Throughput / Latência** | Médio, com alta alocação de memória por chamada | Máximo, próximo ao desempenho puro de sockets |
| **Adequação a Microsserviços** | Pesado e cerimonioso | Ideal para microsserviços, FaaS e APIs REST |

---

## 3. Estrutura dos Projetos

```
concepts/32-minimal-apis/
├── README.md
├── framework/
│   ├── MinimalApisDemo.csproj
│   ├── Web.config
│   ├── Global.asax / Global.asax.cs
│   ├── App_Start/
│   │   ├── RouteConfig.cs
│   │   └── WebApiConfig.cs
│   ├── Models/
│   │   └── TarefaLegada.cs
│   ├── Controllers/
│   │   ├── TarefasApiController.cs
│   │   └── TarefasLegadoController.cs
│   └── Views/
│       └── TarefasLegado/Index.cshtml
└── net10/
    ├── MinimalApisDemo.csproj
    ├── Program.cs
    ├── Models/
    │   └── Tarefa.cs
    ├── Endpoints/
    │   └── TarefasEndpoints.cs
    ├── Filters/
    │   └── ValidacaoTarefaFilter.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 8200)
```powershell
cd concepts/32-minimal-apis/net10
dotnet run
```
Acesse no navegador: `http://localhost:8200`

### .NET Framework 4.8.1 (Porta 8201)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\32-minimal-apis\framework /port:8201
```
Acesse no navegador: `http://localhost:8201`
