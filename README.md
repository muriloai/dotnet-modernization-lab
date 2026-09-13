# .NET Modernization Lab

Laboratório prático de modernização de código comparando, lado a lado, o ecossistema legado **.NET Framework 4.8.1** e ecossistema moderno do **.NET 10**.

O objetivo deste repositório é demonstrar como funcionalidades equivalentes eram construídas no modelo clássico do ASP.NET e como são implementadas com os recursos atuais da plataforma .NET e do C# 14.

---

## Arquitetura do Repositório: Conceitos Isolados

Cada conceito técnico possui sua própria pasta independente dentro do diretório `concepts/`, contendo dois mini-projetos funcionais:

```
concepts/
├── 01-project-structure/
│   ├── README.md               # Explicação técnica e comparativo do conceito
│   ├── framework/              # Projeto ASP.NET (.NET Framework 4.8.1)
│   └── net10/                  # Projeto ASP.NET Core (.NET 10)
├── 02-configuration/
│   ├── README.md
│   ├── framework/
│   └── net10/
└── ...
```

Desta forma, cada laboratório pode ser compilado, executado e inspecionado individualmente, sem interferência de outros módulos.

---

## Módulos e Conceitos do Laboratório

### Fundamentos da Plataforma

| Conceito                                      | Nome                                 | Tecnologias no Legado (4.8.1)                                 | Tecnologias no Moderno (.NET 10)                                        | Status       |
| --------------------------------------------- | ------------------------------------ | ------------------------------------------------------------- | ----------------------------------------------------------------------- | ------------ |
| [01](concepts/01-project-structure/README.md) | Estrutura de Projeto e Inicialização | MSBuild clássico XML, `Global.asax.cs`, IIS                   | SDK-style csproj, `Program.cs` com Top-Level, Kestrel                   | Concluído    |
| [02](concepts/02-configuration/README.md)     | Configuração e Options Pattern       | `ConfigurationManager`, XML `Web.config`, seções customizadas | `IConfiguration`, JSON hierárquico, Options Pattern e `IOptionsMonitor` | Concluído    |
| [03](concepts/03-dependency-injection/README.md) | Injeção de Dependências             | Sem DI nativo, containers externos e Service Locator          | Container de injeção nativo, ciclos de vida e Keyed Services            | Concluído    |
| [04](concepts/04-middleware-vs-modules/README.md) | Middlewares vs Módulos HTTP         | IHttpModule, IHttpHandler e eventos do IIS                    | Pipeline linear de middlewares com RequestDelegate                      | Concluído    |
| [05](concepts/05-routing/README.md)           | Roteamento                           | Tabelas separadas (MVC vs Web API) e conflitos de convenção   | Endpoint Routing unificado, MapGroup e restrições tipadas                | Concluído    |

---

## Pré-requisitos para Execução

### Para os projetos em .NET 10:

- [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado.
- Terminal compatível com a CLI `dotnet` (PowerShell, Bash ou Prompt de Comando).
- Editor ou IDE de sua preferência (Visual Studio 2022, Visual Studio Code ou JetBrains Rider).

### Para os projetos em .NET Framework 4.8.1:

- Sistema Operacional Windows.
- [Visual Studio 2022](https://visualstudio.microsoft.com/) com a carga de trabalho **ASP.NET e desenvolvimento web** instalada (necessário para o runtime IIS Express e referências do MSBuild clássico).
- Developer Pack do .NET Framework 4.8.1.

---

## Como Executar os Laboratórios

### Executando o lado moderno (.NET 10)

Abra o terminal na pasta do conceito desejado e utilize os comandos da CLI:

```bash
cd concepts/01-project-structure/net10
dotnet run
```

### Executando o lado legado (.NET Framework 4.8.1)

1. Abra o arquivo de projeto (`.csproj`) ou a solução correspondente no Visual Studio 2022.
2. Defina o projeto legado como projeto de inicialização.
3. Pressione `Ctrl + F5` para compilar e inicializar no IIS Express.

Para detalhes específicos de cada laboratório, consulte o arquivo `README.md` localizado na pasta de cada conceito.
