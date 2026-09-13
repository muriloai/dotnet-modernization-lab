# Lab 02: Configuração e Options Pattern

> Comparativo prático entre o modelo de configuração estático e baseado em XML (`Web.config` + `ConfigurationManager`) do **.NET Framework 4.8.1** e o modelo moderno, hierárquico, fortemente tipado e recarregável (`appsettings.json` + `IConfiguration` + Options Pattern) do **.NET 10**.

---

## O que é este conceito?

O sistema de configuração é responsável por fornecer à aplicação parâmetros variáveis de acordo com o ambiente de execução, tais como strings de conexão de banco de dados, chaves de API, portas de serviços e limites operacionais.

- No **.NET Framework (1.0 até 4.8.1)**, a configuração é centralizada no arquivo XML `Web.config` e acessada globalmente através da classe estática `ConfigurationManager`. Para criar seções customizadas e agrupadas, o desenvolvedor precisava criar classes herdando de `ConfigurationSection` e mapear elementos XML manualmente.
- No **.NET 10 (e no .NET moderno)**, a configuração é baseada em múltiplos provedores flexíveis (arquivos JSON, variáveis de ambiente, argumentos de linha de comando, secrets). Os dados são mapeados diretamente para classes C# simples (POCO) através do **Options Pattern**, com suporte a validação na inicialização da aplicação e recarregamento automático em memória sem necessidade de reiniciar o processo.

---

## Por que mudou?

1. **Eliminação do XML Monolítico:** O formato XML do `Web.config` é verboso e complexo de manipular em esteiras de integração contínua (CI/CD) e contêineres Docker. O JSON e as variáveis de ambiente facilitam a configuração moderna orientada a nuvem.
2. **Fim do Estado Estático Global:** A classe `ConfigurationManager` é estática e lê diretamente de arquivos de sistema, o que dificulta testes de unidade. No .NET 10, a configuração é injetada via injeção de dependências, permitindo simular qualquer cenário de teste com facilidade.
3. **Tipagem Forte sem Cerimônia:** No .NET Framework, `ConfigurationManager.AppSettings["Chave"]` sempre retorna uma `string` não tipada. Se a chave não existir ou tiver formato inválido, erros de conversão ocorrem em tempo de execução. Com o Options Pattern do .NET 10, os valores são vinculados diretamente a propriedades tipadas (`int`, `bool`, objetos aninhados).
4. **Validação Antecipada (Fail-Fast):** O .NET moderno permite validar as opções logo na subida da aplicação com `ValidateDataAnnotations()` e `ValidateOnStart()`. Se uma porta for inválida ou uma URL obrigatória estiver ausente, a aplicação falha imediatamente no início, evitando comportamentos inesperados em produção.
5. **Recarregamento Dinâmico (Hot Reload de Configuração):** No ecossistema antigo, qualquer alteração no `Web.config` reiniciava imediatamente o processo de trabalho do IIS (`w3wp.exe`), descartando sessões em memória. No .NET 10, o `IOptionsMonitor<T>` detecta alterações no arquivo e atualiza os valores em memória instantaneamente, sem reiniciar a aplicação.

---

## Comparativo Direto

| Dimensão Técnica | .NET Framework 4.8.1 (Legado) | .NET 10 (Moderno) |
|---|---|---|
| **Formato Principal** | XML (`Web.config`, `App.config`) | JSON (`appsettings.json`), Variáveis de Ambiente, Secrets |
| **Acesso Básico** | `ConfigurationManager.AppSettings["Chave"]` (retorna `string`) | `IConfiguration["Secao:Chave"]` ou `IConfiguration.GetValue<T>()` |
| **Padrão Recomendado** | Acesso estático direto ou seções XML | **Options Pattern** (`IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`) |
| **Seções Personalizadas** | Classes complexas herdando de `ConfigurationSection` | Classes POCO simples vinculadas via `.Bind()` |
| **Múltiplos Ambientes** | Transformações XML com sintaxe XDT (`Web.Release.config`) | Convenção de arquivos (`appsettings.Development.json`) e variáveis de ambiente |
| **Validação** | Código defensivo manual no momento do uso | DataAnnotations com `.ValidateDataAnnotations().ValidateOnStart()` |
| **Recarregamento a Quente** | Reinício obrigatório do processo do IIS | Nativo e sem reinício via `IOptionsMonitor<T>` |
| **Testabilidade** | Difícil por depender da classe estática `ConfigurationManager` | Simples, pois classes recebem `IOptions<T>` via injeção de dependências |

---

## Ciclos de Vida do Options Pattern no .NET 10

O .NET moderno oferece três interfaces com propósitos específicos para consumo de configurações:

1. **`IOptions<T>` (Singleton):**
   - Registrado como Singleton.
   - Lê os dados uma única vez na inicialização da aplicação.
   - Possui o menor overhead de performance, mas não reflete alterações feitas no arquivo de configuração após a subida.

2. **`IOptionsSnapshot<T>` (Scoped):**
   - Registrado como Scoped (por requisição HTTP).
   - Recalcula e lê os valores a cada requisição, garantindo que a mesma requisição veja dados consistentes.
   - Ideal para quando as opções precisam refletir mudanças recentes em cenários com requisições isoladas.

3. **`IOptionsMonitor<T>` (Singleton Reativo):**
   - Registrado como Singleton com suporte a notificações de evento (`OnChange`).
   - Atualiza a propriedade `CurrentValue` imediatamente quando o arquivo de configuração subjacente for alterado, sem reiniciar a aplicação.

---

## Estrutura de Arquivos dos Projetos

### Projeto Legado: `framework/`
```
framework/
├── ConfigurationDemo.csproj          # Projeto no formato clássico MSBuild (.NET Framework 4.8.1)
├── packages.config                  # Pacotes NuGet do ASP.NET MVC 5
├── Web.config                       # Configurações XML: appSettings, connectionStrings e seções customizadas
├── Global.asax                      # Declaração do ciclo de vida da aplicação
├── Global.asax.cs                   # Registro de rotas no Application_Start
├── App_Start/
│   └── RouteConfig.cs               # Roteamento tradicional do MVC
├── Models/
│   └── ConfiguracaoSmtpSection.cs   # Implementação clássica de ConfigurationSection
├── Controllers/
│   └── ConfigController.cs          # Leitura via ConfigurationManager
├── Views/
│   ├── _ViewStart.cshtml            # Definição do layout padrão
│   ├── Web.config                   # Configuração interna das views Razor
│   ├── Shared/
│   │   └── _Layout.cshtml           # Layout HTML base
│   └── Config/
│       └── Index.cshtml             # Visualização dos dados carregados
├── Content/
│   └── Site.css                     # Estilos visuais
└── Properties/
    └── AssemblyInfo.cs              # Metadados do assembly legado
```

### Projeto Moderno: `net10/`
```
net10/
├── ConfigurationDemo.csproj         # SDK-style minimalista visando net10.0
├── Program.cs                       # Registro do Options Pattern, validações e endpoints
├── appsettings.json                 # Configurações padrão hierárquicas em JSON
├── appsettings.Development.json     # Sobrescrita específica para desenvolvimento local
├── Models/
│   ├── ConfiguracaoGeralOptions.cs  # Classe POCO com DataAnnotations para regras gerais
│   └── SmtpOptions.cs               # Classe POCO com DataAnnotations para envio de e-mails
├── Properties/
│   └── launchSettings.json          # Perfis de inicialização Kestrel (porta 5200)
└── wwwroot/
    └── css/
        └── site.css                 # Estilos visuais compartilhados
```

---

## Como Executar

### Versão Moderna (.NET 10)
Navegue até a pasta `net10/` e utilize a CLI do .NET:

```bash
cd concepts/02-configuration/net10
dotnet run
```

Abra o navegador no endereço indicado (por padrão: `http://localhost:5200`).

Endpoints didáticos disponíveis:
- `GET /`: Painel visual didático comparando as opções carregadas.
- `GET /api/config/direta`: Leitura direta via `IConfiguration["ConfiguracaoGeral:NomeSistema"]`.
- `GET /api/config/options`: Dados lidos através de `IOptions<ConfiguracaoGeralOptions>`.
- `GET /api/config/snapshot`: Dados lidos através de `IOptionsSnapshot<ConfiguracaoGeralOptions>`.
- `GET /api/config/monitor`: Dados lidos através de `IOptionsMonitor<ConfiguracaoGeralOptions>`.

**Teste de Hot Reload no .NET 10:**
1. Com a aplicação rodando, abra o arquivo `concepts/02-configuration/net10/appsettings.json`.
2. Altere o valor de `LimiteItensPorPagina` de `50` para `80`.
3. Salve o arquivo.
4. Recarregue o endpoint `/api/config/monitor` ou a página principal e observe o novo valor atualizado instantaneamente, sem necessidade de parar ou reiniciar o comando `dotnet run`.

---

### Versão Legada (.NET Framework 4.8.1)
1. Abra a solução ou o projeto `ConfigurationDemo.csproj` no **Visual Studio 2022**.
2. Defina o projeto como inicialização e pressione **Ctrl + F5** para executar via IIS Express.
3. Acesse `http://localhost:5201/` para inspecionar os valores carregados pelo `ConfigurationManager`.

Para compilar apenas via terminal (requer MSBuild do Windows):
```cmd
msbuild concepts\02-configuration\framework\ConfigurationDemo.csproj /p:Configuration=Debug
```

---

## O que observar neste laboratório?

1. **Diferença de complexidade para seções customizadas:** Compare o arquivo `ConfiguracaoSmtpSection.cs` (Framework) com a classe `SmtpOptions.cs` (.NET 10). No Framework são necessárias dezenas de linhas de herança e atributos XML, enquanto no .NET 10 é uma classe POCO direta.
2. **Validação na Inicialização:** No .NET 10, experimente colocar uma porta inválida (ex: `0` ou `70000`) em `appsettings.json` e iniciar a aplicação. Ela falhará imediatamente no startup com uma mensagem explicativa do `ValidateDataAnnotations()`, impedindo que um erro chegue à produção.
3. **Ausência de Recarregamento no Legado:** No Framework, alterar o `Web.config` força uma reinicialização de todo o domínio de aplicação (`AppDomain`), o que pode interromper conexões ativas. No .NET 10, o `IOptionsMonitor` atualiza o estado sem qualquer reinício de processo.
