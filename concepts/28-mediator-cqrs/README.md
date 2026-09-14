# Conceito 28: Mediator e CQRS (Command Query Responsibility Segregation)

Este laboratório compara a arquitetura tradicional baseada em servicos monoliticos (Fat Services) no .NET Framework 4.8.1 com o padrao Mediator e CQRS (com Pipeline Behaviors) no .NET 10.

---

## 1. Cenário e Justificativa

Em aplicacoes corporativas que crescem ao longo dos anos, camadas de servico frequentemente se transformam em classes monoliticas com dezenas de responsabilidades conflitantes.

### No .NET Framework 4.8.1 (Legado)
* **Fat Services / God Services:** Interfaces como `IPedidoService` concentravam metodos de criacao, faturamento, calculo de frete, relatorios analiticos e consultas paginadas em uma unica classe com centenas ou milhares de linhas.
* **Acoplamento Alto:** Modificar a regra de criacao de pedidos exigia alterar uma classe compartilhada por todo o sistema, aumentando o risco de regressao em consultas e relatorios.
* **Repeticao de Cross-Cutting Concerns:** Validacao, logs de auditoria, medicao de tempo e tratamento de transacoes precisavam ser repetidos manualmente dentro de cada metodo do servico ou acoplados via interceptores complexos de terceiros.
* **Falta de Segregacao de Modelos:** As mesmas entidades de dominio ou DataSets eram reutilizados para leitura e escrita, gerando sobrecarga de dados desnecessarios em consultas de tela.

### No .NET 10 (Moderno)
* **CQRS (Command Query Responsibility Segregation):** Segregacao explicita entre operacoes de escrita (Commands) que alteram estado e operacoes de leitura (Queries) que retornam DTOs otimizados.
* **Padrao Mediator:** O controller nao conhece o servico concreto nem a camada de persistencia; ele apenas despacha a intencao (`await _mediator.Send(command)` ou `await _mediator.Send(query)`).
* **Vertical Slice Architecture:** Cada caso de uso (ex: `CriarPedidoCommand`, `ObterPedidosQuery`) e uma fatia independente e isolada, facilitando manutencao, testes unitarios e evolucao sem efeitos colaterais.
* **Pipeline Behaviors:** Middleware in-process para Mediator. Intercepta todas as requisicoes para aplicar logs estruturados, medicao de tempo de execucao e validacao automatica antes de atingir o Handler de negocio.

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Organizacao de Regras** | Servicos monoliticos (`PedidoService`) | Casos de uso segregados em Commands e Queries |
| **Acoplamento no Controller** | Injecao direta de servicos com muitos metodos | Dependencia exclusiva da abstracao `IMediator` |
| **Cross-Cutting Concerns** | Codigo duplicado em cada metodo ou AOP legado | Pipeline Behaviors genericos (`IPipelineBehavior<TRequest, TResponse>`) |
| **Modelo de Leitura** | Entidades pesadas ou DataSets com campos extras | DTOs imutaveis (records) projetados especificamente para a UI |
| **Testabilidade** | Mocks gigantes de interfaces com dezenas de metodos | Teste isolado do Handler de cada Command ou Query individual |
| **Injecao de Dependencias** | Containers legados com configuracao manual | DI nativa do .NET com registro automatico de Handlers |

---

## 3. Estrutura dos Projetos

```
concepts/28-mediator-cqrs/
├── README.md
├── framework/
│   ├── PedidosCqrsDemo.csproj
│   ├── Global.asax / Global.asax.cs
│   ├── Web.config
│   ├── Models/
│   │   └── PedidoLegado.cs
│   ├── Services/
│   │   ├── IPedidoService.cs
│   │   └── PedidoMonoliticoService.cs
│   ├── Controllers/
│   │   └── PedidosLegadoController.cs
│   └── Views/
│       └── PedidosLegado/Index.cshtml
└── net10/
    ├── PedidosCqrsDemo.csproj
    ├── Program.cs
    ├── Features/Pedidos/
    │   ├── Commands/
    │   │   ├── CriarPedidoCommand.cs
    │   │   └── CriarPedidoHandler.cs
    │   ├── Queries/
    │   │   ├── ObterPedidosQuery.cs
    │   │   └── ObterPedidosHandler.cs
    │   └── DTOs/
    │       ├── CriarPedidoRequest.cs
    │       └── PedidoDetalheDto.cs
    ├── Infrastructure/
    │   ├── Mediator/
    │   │   ├── IMediator.cs
    │   │   ├── IRequest.cs
    │   │   ├── IRequestHandler.cs
    │   │   ├── IPipelineBehavior.cs
    │   │   └── MediatorDispatcher.cs
    │   ├── Behaviors/
    │   │   ├── LoggingBehavior.cs
    │   │   └── TimingBehavior.cs
    │   └── Persistence/
    │       └── PedidoRepositoryEmMemoria.cs
    ├── Controllers/
    │   └── PedidosController.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 7800)
```powershell
cd concepts/28-mediator-cqrs/net10
dotnet run
```
Acesse no navegador: `http://localhost:7800`

### .NET Framework 4.8.1 (Porta 7801)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\28-mediator-cqrs\framework /port:7801
```
Acesse no navegador: `http://localhost:7801`
