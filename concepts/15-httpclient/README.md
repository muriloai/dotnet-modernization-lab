# Conceito 15: HttpClient e Requisições Externas

Este laboratório compara o consumo de serviços HTTP externos no **.NET Framework 4.8.1 (WebClient, HttpWebRequest e o padrão perigoso de instanciar HttpClient diretamente)** com as práticas recomendadas no **.NET 10 (IHttpClientFactory, Typed Clients, SocketsHttpHandler e resiliência)**.

---

## 1. Visão Geral e Contexto Histórico

O consumo de APIs HTTP no ecossistema .NET passou por quatro grandes fases:
1. **WebClient e HttpWebRequest (.NET 1.0 a 4.0):** Classes legadas e síncronas que bloqueavam threads do IIS, com configuração global em `ServicePointManager`.
2. **Introdução do HttpClient (.NET 4.5):** API baseada em `Task` assíncrona, mas implementando `IDisposable`. Essa interface induziu milhões de desenvolvedores ao erro de utilizá-la com o bloco `using`.
3. **IHttpClientFactory (ASP.NET Core 2.1 em diante):** Abstração que separa a vida útil do `HttpClient` do ciclo de vida de seus manipuladores de conexão (`HttpMessageHandler`).
4. **SocketsHttpHandler e Resiliência (.NET 8 a 10):** Pipeline HTTP reescrito em código gerenciado de altíssima performance, com suporte nativo a HTTP/2 e HTTP/3, e integração direta com estratégias de resiliência.

---

## 2. A Armadilha Clássica: Socket Exhaustion vs DNS Stale

### O Problema do Socket Exhaustion (Exaustão de Portas)
No .NET Framework, era comum encontrar código como:
```csharp
// ANTI-PADRÃO: Causa exaustão de sockets sob carga
public async Task<string> ObterCotacaoAsync()
{
    using (var client = new HttpClient())
    {
        return await client.GetStringAsync("https://api.externa.com/cotacao");
    }
}
```
Mesmo após o encerramento do bloco `using`, a conexão TCP subjacente não é fechada imediatamente pelo sistema operacional: ela entra no estado `TIME_WAIT` (geralmente por 120 a 240 segundos). Sob carga moderada ou alta, todas as portas efêmeras disponíveis são esgotadas, resultando em:
`System.Net.Sockets.SocketException: Only one usage of each socket address (protocol/network address/port) is normally permitted.`

### O Problema do DNS Stale (DNS Desatualizado)
Para contornar o esgotamento de portas, recomendava-se transformar o `HttpClient` em uma instância estática ou singleton:
```csharp
// SOLUÇÃO INCOMPLETA: Evita socket exhaustion, mas ignora mudanças de DNS
private static readonly HttpClient _client = new HttpClient();
```
Essa abordagem mantinha a conexão TCP aberta indefinidamente. Se o serviço externo alterasse seu endereço IP (por exemplo, durante rotação de balanceadores de carga ou failover de nuvem), a aplicação continuava enviando requisições para o IP antigo até o processo ser reiniciado, a menos que o desenvolvedor configurasse o obscuro `ServicePointManager.FindServicePoint(uri).ConnectionLeaseTimeout`.

### A Solução Definitiva com IHttpClientFactory no .NET 10
O `IHttpClientFactory` resolve ambos os problemas:
- Mantém uma fila de manipuladores (`HttpMessageHandler`) reciclados a cada 2 minutos (tempo padrão).
- Cria instâncias leves de `HttpClient` sob demanda sem custo de conexões duplicadas.
- Quando o tempo de vida do manipulador expira, ele não é fechado abruptamente: aguarda as requisições em andamento terminarem e força a abertura de uma nova conexão, revalidando o DNS.

---

## 3. Comparativo Técnico: Legado vs Moderno

| Característica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
| :--- | :--- | :--- |
| **Padrão de Criação** | `new HttpClient()` com `using` ou `static` | `IHttpClientFactory` gerenciado pelo container de DI |
| **Ciclo de Vida do Handler** | Preso à instância ou indefinido | Pool gerenciado com expiração e reciclagem de DNS |
| **Configuração de Clientes** | Instanciação procedural manual | Typed Clients e Named Clients com injeção automática |
| **Resiliência e Retry** | Código procedural manual com loops de `try/catch` | Handlers de resiliência e políticas integradas |
| **Serialização Integrada** | `Newtonsoft.Json` manual em buffers de string | `System.Net.Http.Json` com streams de alta performance |
| **Controle de Conexões** | `ServicePointManager` estático global | `SocketsHttpHandler` configurável por cliente |
| **Protocolos HTTP** | HTTP 1.1 prioritário, sem HTTP/3 | HTTP/1.1, HTTP/2 e HTTP/3 nativos com pooling |

---

## 4. Estrutura do Laboratório

```text
concepts/15-httpclient/
├── README.md
├── net10/
│   ├── Properties/launchSettings.json   (Porta Kestrel 6500)
│   ├── Models/                          (DTOs de retorno e modelos de métricas)
│   ├── Services/
│   │   ├── ICotacaoService.cs           (Contrato do cliente tipado)
│   │   ├── CotacaoService.cs            (Typed Client consumindo System.Net.Http.Json)
│   │   ├── ISimuladorFalhasService.cs
│   │   └── SimuladorFalhasService.cs    (API mock local para testes de retry e timeout)
│   ├── Controllers/
│   │   ├── ClientesHttpController.cs    (Compara Basic, Named e Typed Client)
│   │   └── MockCotacoesController.cs    (Endpoint mock para simular sucessos e falhas)
│   ├── wwwroot/                         (Painel interativo para monitoramento de conexões)
│   ├── Program.cs                       (AddHttpClient tipado, nomeado e configuração de pooling)
│   └── HttpClientDemo.csproj
└── framework/
    ├── Properties/AssemblyInfo.cs
    ├── App_Start/RouteConfig.cs
    ├── App_Start/WebApiConfig.cs
    ├── Controllers/Api/ClientesHttpApiController.cs (Demonstra new HttpClient vs static)
    ├── Controllers/HomeController.cs
    ├── Models/
    ├── Services/
    │   └── CotacaoLegadaService.cs      (Demonstra o anti-padrão using e a abordagem estática)
    ├── Views/Home/Index.cshtml          (Painel interativo legado)
    ├── Content/Site.css
    ├── Global.asax / Global.asax.cs
    ├── Web.config                       (Porta IIS Express 6501)
    ├── packages.config
    └── HttpClientDemo.csproj
```

---

## 5. Como Executar os Projetos

### Projeto Moderno (.NET 10)
1. Navegue até o diretório `concepts/15-httpclient/net10/`.
2. Execute a aplicação via CLI:
   ```bash
   dotnet run
   ```
3. Acesse o painel interativo no navegador:
   ```text
   http://localhost:6500
   ```
4. Teste as operações de cliente básico, cliente nomeado, cliente tipado e simulação de resiliência com métricas de tempo e reaproveitamento de conexão.

### Projeto Legado (.NET Framework 4.8.1)
1. Abra o arquivo de solução ou o projeto `HttpClientDemo.csproj` localizado em `concepts/15-httpclient/framework/` no Visual Studio 2022.
2. Inicie a execução sob o IIS Express (configurado para a porta `6501`).
3. Acesse a interface no navegador:
   ```text
   http://localhost:6501
   ```
