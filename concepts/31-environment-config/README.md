# Conceito 31: Ambientes e Feature Flags

Este laboratório compara o gerenciamento de múltiplos ambientes e o controle de funcionalidades (Feature Flags) entre o .NET Framework 4.8.1 e o .NET 10.

---

## 1. Cenário e Justificativa

O gerenciamento de ambientes e o acionamento dinâmico de funcionalidades são vitais para continuous delivery, testes A/B, lançamentos graduais (canary releases) e contenção de incidentes (kill switch).

### No .NET Framework 4.8.1 (Legado)
* **Ambientes Estáticos:** Ambientes eram frequentemente definidos via diretivas de pré-processador (`#if DEBUG`) ou transformações XML em tempo de build (`Web.Release.config`). Alterar o comportamento em runtime exigia reinstalação ou reinicialização.
* **Feature Toggles em appSettings:** Flags eram lidas de strings simples no `Web.config` (`ConfigurationManager.AppSettings["FeatureNova"] == "true"`), sem tipagem e sem validação estruturada.
* **Reciclagem do IIS em Mudanças:** Qualquer edição no `Web.config` em produção causava a reciclagem imediata do Application Pool (w3wp.exe), abortando requisições em trânsito e zerando sessões em memória.
* **Ausência de Filtros Contextuais:** Não havia suporte nativo a ativação por percentual de usuários, janelas de horário ou perfis específicos de clientes.

### No .NET 10 (Moderno)
* **Resolução Dinâmica com IWebHostEnvironment:** A variável de ambiente `ASPNETCORE_ENVIRONMENT` define o ambiente em execução (`IsDevelopment()`, `IsStaging()`, `IsProduction()`).
* **Hierarquia de Configuração em Camadas:** Sobrescrita automática e limpa: `appsettings.json` -> `appsettings.{Environment}.json` -> Variáveis de Ambiente -> Provedores Remotos (Azure App Configuration).
* **Microsoft.FeatureManagement.AspNetCore:** Framework oficial da Microsoft para controle de funcionalidades:
  * Injeção de `IFeatureManager` para controle programático assíncrono.
  * Filtros de ação declarativos (`[FeatureGate("NovoCheckout")]`) que protegem endpoints automaticamente.
  * Recarregamento a quente (hot reload) sem reiniciar a aplicação ou derrubar conexões Kestrel.
  * Suporte extensível para regras contextuais, como percentual de usuários e janelas temporais.

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Identificação de Ambiente** | `#if DEBUG` ou transformações XDT | `IWebHostEnvironment` e `ASPNETCORE_ENVIRONMENT` |
| **Fontes de Configuração** | Arquivo `Web.config` monolítico | `appsettings.json`, variáveis de ambiente, provedores de nuvem |
| **Biblioteca de Feature Flags** | Nenhuma nativa (código manual ad-hoc) | `Microsoft.FeatureManagement.AspNetCore` |
| **Proteção Declarativa** | Filtros customizados manuais | Atributo oficial `[FeatureGate("FeatureName")]` |
| **Alteração em Runtime** | Recicla o AppPool do IIS (derruba requisições) | Recarregamento a quente transparente (zero downtime) |
| **Filtros Avançados** | Requer desenvolvimento proprietário complexo | Suporte nativo a PercentageFilter, TimeWindowFilter e Targeting |

---

## 3. Estrutura dos Projetos

```
concepts/31-environment-config/
├── README.md
├── framework/
│   ├── EnvironmentDemo.csproj
│   ├── Web.config
│   ├── Global.asax / Global.asax.cs
│   ├── Controllers/
│   │   └── EnvironmentLegadoController.cs
│   └── Views/
│       └── EnvironmentLegado/Index.cshtml
└── net10/
    ├── EnvironmentDemo.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── appsettings.Production.json
    ├── Models/
    │   └── FeatureStatusDto.cs
    ├── Services/
    │   └── GerenciadorFlagsEmMemoria.cs
    ├── Controllers/
    │   ├── CheckoutController.cs
    │   └── FeatureManagerController.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 8100)
```powershell
cd concepts/31-environment-config/net10
dotnet run
```
Acesse no navegador: `http://localhost:8100`

Para simular o ambiente de Produção:
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run
```

### .NET Framework 4.8.1 (Porta 8101)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\31-environment-config\framework /port:8101
```
Acesse no navegador: `http://localhost:8101`
