# Conceito 25: Evolução de Async e Await

Este laboratório encerra o módulo de **Evolução da Linguagem C#**, analisando a evolução da programação assíncrona entre o **.NET Framework 4.8.1** e o **.NET 10**, com foco na eliminação de deadlocks de sincronização, introdução de `ValueTask<T>`, streaming com `IAsyncEnumerable<T>` e combinadores modernos como `Task.WhenEach`.

---

## 1. Contexto e Motivação

A introdução de `async` e `await` no C# 5.0 revolucionou a escrita de código assíncrono. Porém, no .NET Framework tradicional, a coexistência com a arquitetura herdada do ASP.NET e IIS gerou armadilhas graves de concorrência e alocação de recursos.

### No .NET Framework 4.8.1 (Legado):
- **O Fantasma do AspNetSynchronizationContext:** O ASP.NET clássico gerenciava requisições com afinidade de contexto de sincronização. Se qualquer trecho de código bloqueasse uma Task de forma síncrona com `.Result`, `.Wait()` ou `GetAwaiter().GetResult()`, o sistema entrava em **Deadlock**. A thread da requisição ficava bloqueada aguardando a Task terminar, enquanto a Task aguardava o contexto da mesma thread ser liberado para continuar.
- **Proliferação de ConfigureAwait(false):** Para prevenir deadlocks em bibliotecas e camadas de negócio, os desenvolvedores eram obrigados a adicionar `.ConfigureAwait(false)` em cada chamada assíncrona, poluindo a legibilidade do código.
- **Ausência de Streaming Assíncrono:** Não existia `IAsyncEnumerable<T>`. Para retornar múltiplos registros assíncronos de um banco de dados ou serviço externo, era obrigatório materializar a lista inteira na memória do servidor (`Task<List<T>>`) antes de entregar o primeiro byte ao cliente.
- **Sobrecarga de Alocação de Task:** Cada método assíncrono que retornava `Task<T>` forçava uma alocação de objeto na Heap, mesmo nos cenários onde o valor já estava imediatamente disponível em cache.
- **Perigo de async void:** Métodos marcados como `async void` (exceto manipuladores de eventos de UI) não podiam ter suas exceções capturadas por blocos `try/catch` convencionais, provocando o encerramento abrupto do processo de trabalho (w3wp.exe) do IIS.

### No .NET 10 (Moderno):
- **Eliminação do SynchronizationContext no ASP.NET Core:** O ASP.NET Core foi projetado do zero sem nenhum `SynchronizationContext`. As continuations após o `await` são executadas livremente em qualquer thread disponível do ThreadPool. O uso de `.ConfigureAwait(false)` em controllers e serviços de aplicação tornou-se desnecessário.
- **ValueTask e ValueTask<T> (Zero-Allocation em Retorno Imediato):** Quando um método assíncrono obtém o dado diretamente da memória (cache hit) ou executa sem bloqueio real de I/O, `ValueTask<T>` retorna o valor por cópia de struct na pilha, eliminando a alocação de um objeto `Task` na Heap.
- **Streaming Assíncrono com IAsyncEnumerable<T> e await foreach:** Permite produzir e consumir itens assincronamente um a um conforme chegam da rede ou do banco, viabilizando transferências de grandes volumes de dados com pegada de memória constante e mínima.
- **Task.WhenEach:** Novo combinador assíncrono que permite iterar sobre um conjunto de tarefas paralelas à medida que cada uma conclui, sem esperar que todas terminem como no `Task.WhenAll`.

---

## 2. Comparativo Técnico Estruturado

| Aspecto | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Contexto de Sincronização** | `AspNetSynchronizationContext` propenso a deadlocks com `.Result` | Sem `SynchronizationContext` no ASP.NET Core |
| **Uso de ConfigureAwait(false)** | Essencial em camadas inferiores para evitar travamento de threads | Dispensável no código de aplicação ASP.NET Core |
| **Retorno Síncrono de Método Async** | Aloca uma nova instância de `Task<T>` na Heap | `ValueTask<T>` retorna como struct na pilha sem alocação |
| **Transmissão de Coleções** | `Task<List<T>>` materializa todos os dados antes do envio | `IAsyncEnumerable<T>` transmite elemento a elemento |
| **Consumo de Streams** | Loops bloqueantes ou buffers intermediários | Sintaxe nativa com `await foreach` |
| **Processamento Paralelo sob Demanda** | Requer `Task.WhenAny` em loop manual complexo | Combinador nativo `Task.WhenEach` |

---

## 3. Estrutura dos Projetos

```text
concepts/25-async-await/
├── README.md
├── framework/                              # Projeto Legado (.NET Framework 4.8.1)
│   ├── App_Start/
│   │   └── RouteConfig.cs
│   ├── Content/
│   │   └── Site.css
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Global.asax
│   │   └── Global.asax.cs
│   ├── Models/
│   │   └── EventoStreamLegado.cs
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Services/
│   │   └── OperacoesAssincronasLegadoService.cs
│   ├── Views/
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml
│   │   ├── Web.config
│   │   └── _ViewStart.cshtml
│   ├── AsyncAwaitDemo.csproj
│   ├── Web.config
│   └── packages.config
└── net10/                                  # Projeto Moderno (.NET 10)
    ├── Controllers/
    │   └── AsyncAwaitController.cs
    ├── Models/
    │   └── EventoStream.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Services/
    │   └── OperacoesAssincronasModernoService.cs
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── AsyncAwaitDemo.csproj
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
| `net10/` | .NET 10 | `http://localhost:7500` | Kestrel |
| `framework/` | .NET Framework 4.8.1 | `http://localhost:7501` | IIS Express |

---

## 5. Como Executar os Laboratórios

### Executando o Projeto Moderno (.NET 10)
```bash
cd concepts/25-async-await/net10
dotnet run
```
Abra `http://localhost:7500` para experimentar:
1. **Streaming Contínuo com IAsyncEnumerable<T>:** Veja itens gerados assincronamente e recebidos em tempo real pelo navegador.
2. **Comparação de Alocações Task vs ValueTask:** Inspecione a eliminação de alocações na Heap quando um dado é servido da memória de forma imediata.
3. **Processamento Assíncrono Conforme Conclusão (Task.WhenEach):** Dispare múltiplas chamadas assíncronas com latências variáveis e receba os resultados na ordem exata de finalização.

### Executando o Projeto Legado (.NET Framework 4.8.1)
1. Abra `AsyncAwaitDemo.csproj` no Visual Studio 2022.
2. Inicie com `Ctrl + F5` no IIS Express na porta `7501`.
3. Teste o comportamento de espera síncrona com `Task.Result` e a exigência de buffering completo em coleções `List<T>`.
