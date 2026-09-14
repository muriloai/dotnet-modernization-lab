# Conceito 33: Native AOT e Trimming

Este laboratório compara o modelo de execução tradicional baseado em JIT (Just-In-Time) no .NET Framework 4.8.1 com a compilação nativa Ahead-of-Time (Native AOT) e Trimming no .NET 10.

---

## 1. Cenário e Justificativa

Em arquiteturas modernas de nuvem (como Serverless, contêineres efêmeros e microsserviços de escalonamento rápido), duas métricas são críticas: **tempo de inicialização (Cold Start)** e **pegada de memória de trabalho (Working Set)**.

### No .NET Framework 4.8.1 (Legado)
* **Compilação JIT Obrigatória em Runtime:** O código C# é compilado para Intermediate Language (IL). Na execução, o compilador JIT (RyuJIT) traduz os métodos para código de máquina conforme são chamados pela primeira vez, gerando latência perceptível no início da aplicação.
* **Sobrecarga Permanente de Memória:** O compilador JIT, as tabelas de tipos e metadados completos de todas as DLLs referenciadas precisam ser mantidos na memória RAM de cada processo (w3wp.exe consome tipicamente entre 120 MB e 300 MB mesmo ocioso).
* **NGen Local e Inflexível:** A ferramenta NGen permitia pré-compilar imagens na máquina de destino, mas exigia privilégios de administrador, dependia do estado exato da máquina e não eliminava código morto.
* **Uso Massivo de Reflection Dinâmica:** Bibliotecas dependiam de inspeção em runtime, impedindo a remoção estática de código não utilizado.

### No .NET 10 (Moderno)
* **Compilação Nativa Ahead-of-Time (<PublishAot>):** O código C# e todas as suas dependências são compilados diretamente para um executável binário nativo da arquitetura alvo (x64, ARM64).
* **Sem Compilador JIT em Produção:** O binário final não contém o motor JIT, tornando impossível a injeção ou geração dinâmica de código não verificado, aumentando a segurança.
* **Inicialização Instantânea:** Sem a etapa de tradução JIT, o tempo de inicialização cai para a escala de 8 a 15 milissegundos.
* **Trimming Estático Agressivo (<PublishTrimmed>):** O analisador de árvore de chamadas do SDK remove classes, métodos e propriedades de bibliotecas que nunca são chamados, reduzindo o binário a poucos megabytes.
* **Source Generators Obrigatórios:** Como a Reflection dinâmica é desencorajada ou desabilitada em AOT, geradores de código em tempo de compilação criam serializadores (`JsonSerializerContext`) e injeção de dependências sem custo em runtime.
* **Memória Mínima:** A aplicação em execução consome tipicamente entre 15 MB e 30 MB de RAM.

---

## 2. Comparativo Técnico

| Recurso | .NET Framework 4.8.1 | .NET 10 (Native AOT) |
| :--- | :--- | :--- |
| **Tipo de Compilação** | JIT em tempo de execução (RyuJIT) | Ahead-of-Time (AOT) em tempo de build |
| **Tempo de Inicialização (Cold Start)** | 800 ms a 3500 ms (compilação JIT inicial) | 8 ms a 20 ms (código nativo pronto) |
| **Consumo de Memória Inicial** | 120 MB a 300 MB | 15 MB a 35 MB |
| **Presença do Compilador JIT** | Sim, carregado em cada processo | Não, totalmente removido do binário |
| **Remoção de Código Morto (Trimming)** | Nenhuma (todas as DLLs completas) | Sim, estático e agressivo |
| **Serialização JSON** | Reflection dinâmica (Newtonsoft.Json) | Source Generators (`JsonSerializerContext`) |
| **Ambientes Ideais** | Servidores dedicados permanentes | Serverless, FaaS, Cloud Run, Kubernetes |

---

## 3. Estrutura dos Projetos

```
concepts/33-native-aot/
├── README.md
├── framework/
│   ├── JitOverheadDemo.csproj
│   ├── Web.config
│   ├── Global.asax / Global.asax.cs
│   ├── Controllers/
│   │   └── JitDemoLegadoController.cs
│   └── Views/
│       └── JitDemoLegado/Index.cshtml
└── net10/
    ├── NativeAotDemo.csproj
    ├── Program.cs
    ├── Models/
    │   └── AotMetricsDto.cs
    └── wwwroot/
        ├── index.html
        └── css/site.css
```

---

## 4. Execução dos Projetos

### .NET 10 (Porta 8300)
```powershell
cd concepts/33-native-aot/net10
dotnet run
```
Acesse no navegador: `http://localhost:8300`

Para compilar como Native AOT real (requer compilador C++ / Visual Studio C++ build tools no Windows ou clang/gcc no Linux):
```powershell
dotnet publish -c Release -r win-x64 -o ./publish-aot
```

### .NET Framework 4.8.1 (Porta 8301)
```powershell
# Execução via IIS Express
iisexpress /path:c:\GITHUB\dotnet-modernization-lab\concepts\33-native-aot\framework /port:8301
```
Acesse no navegador: `http://localhost:8301`
