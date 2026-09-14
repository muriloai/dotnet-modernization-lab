# Conceito 30: Publicação e Deploy

Este laboratório compara o ciclo de publicação e empacotamento entre o ASP.NET no .NET Framework 4.8.1 e o ecossistema multiplataforma do .NET 10.

---

## 1. Cenário e Justificativa

A forma como uma aplicação é compilada, empacotada e disponibilizada em produção afeta diretamente o tempo de entrega (CI/CD), os custos de infraestrutura e a capacidade de orquestração em nuvem.

### No .NET Framework 4.8.1 (Legado)
* **Acoplamento ao Windows e IIS:** As aplicações dependiam exclusivamente do Windows Server e do IIS (w3wp.exe).
* **WebDeploy e Cópias de Pasta:** O deploy era comumente feito via MSDeploy (`msdeploy.exe`), perfis `.pubxml` no Visual Studio ou cópia manual de arquivos para pastas físicas em `C:\inetpub\wwwroot`.
* **Dependência de GAC e DLL Hell:** Bibliotecas instaladas no Global Assembly Cache ou versões de runtimes do sistema operacional podiam interferir em aplicações adjacentes no mesmo servidor.
* **Transformações XDT Complexas:** O gerenciamento de ambientes dependia de transformações XML (`Web.Release.config` com `xdt:Transform`), com validação difícil e suscetível a erros silenciosos.
* **Contêineres Ineficientes:** Executar aplicações legadas em contêineres requer imagens do Windows Server Core (`mcr.microsoft.com/dotnet/framework/aspnet:4.8.1`), que ocupam entre 5 GB e 8 GB, possuem tempo de inicialização lento (minutos) e não rodam em clusters Kubernetes Linux convencionais.

### No .NET 10 (Moderno)
* **Multiplataforma por Padrão:** O mesmo código compila e executa nativamente em Linux (x64 e ARM64), macOS e Windows.
* **Modos de Publicação com dotnet publish:**
  * **Framework-Dependent:** Gera binários minúsculos (~5 MB) utilizando o runtime instalado no host.
  * **Self-Contained:** Embute o runtime e todas as dependências no diretório de saída, dispensando instalações prévias no servidor.
  * **Single-File:** Agrupa assemblies, runtime e metadados em um único arquivo binário executável.
  * **ReadyToRun (R2R):** Pré-compila código intermediário (IL) em código nativo para inicialização ultrarrápida, reduzindo a latência do JIT no primeiro request.
* **Contêineres Mínimos (Chiseled e Alpine):** Imagens de runtime baseadas em Ubuntu Chiseled ou Alpine pesam menos de 100 MB (ou ~30 MB com Native AOT), sem pacotes desnecessários, com superfície de ataque mínima e inicialização em milissegundos.
* **PublishContainer sem Docker Daemon:** O SDK do .NET 10 permite gerar imagens de contêiner compatíveis com OCI diretamente via MSBuild (`dotnet publish /t:PublishContainer`).

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 |
| :--- | :--- | :--- |
| **Sistemas Operacionais Suportados** | Apenas Windows Server | Linux (todas as distros), macOS, Windows |
| **Ferramenta de Publicação** | MSBuild + WebDeploy (msdeploy.exe) | CLI nativa `dotnet publish` |
| **Tamanho Típico da Imagem de Contêiner** | 5 GB a 8 GB (Windows Server Core) | 80 MB a 120 MB (Linux Chiseled / Alpine) |
| **Formato de Saída** | Pasta cheia de DLLs e arquivos de configuração | Pasta, Single-File binário único ou imagem OCI |
| **Empacotamento de Runtime** | Depende do .NET Framework global do SO | Opção Self-Contained embute o runtime exato |
| **Pré-compilação** | NGen (pesado, local na máquina) | ReadyToRun (R2R) multiplataforma ou Native AOT |
| **Configuração de Ambientes** | Transformações XML XDT em tempo de build | Variáveis de ambiente, appsettings e IConfiguration em runtime |

---

## 3. Estrutura dos Projetos

```
concepts/30-publish-deployment/
├── README.md
├── framework/
│   ├── PublishDeployDemo.csproj
│   ├── Web.config
│   ├── Web.Release.config
│   ├── Global.asax / Global.asax.cs
│   ├── Controllers/
│   │   └── PublishDeployLegadoController.cs
│   └── Views/
│       └── PublishDeployLegado/Index.cshtml
└── net10/
    ├── PublishDeployDemo.csproj
    ├── Dockerfile
    ├── Program.cs
    ├── Controllers/
    │   └── PublishInfoController.cs
    ├── Models/
    │   └── PublishInfoDto.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 8000)
```powershell
cd concepts/30-publish-deployment/net10
dotnet run
```
Acesse no navegador: `http://localhost:8000`

Para testar os modos de publicação:
```powershell
# Publicação Framework-Dependent
dotnet publish -c Release -o ./publish-fdd

# Publicação Self-Contained Single-File para Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish-linux
```

### .NET Framework 4.8.1 (Porta 8001)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\30-publish-deployment\framework /port:8001
```
Acesse no navegador: `http://localhost:8001`
