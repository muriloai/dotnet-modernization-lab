using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using PublishDeployDemo.Models;

namespace PublishDeployDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishInfoController : ControllerBase
{
    private static readonly DateTime _inicioProcesso = Process.GetCurrentProcess().StartTime.ToUniversalTime();

    [HttpGet("diagnostico")]
    public IActionResult ObterDiagnostico()
    {
        var proc = Process.GetCurrentProcess();
        var memoriaMb = proc.WorkingSet64 / (1024.0 * 1024.0);
        var uptimeSeg = (DateTime.UtcNow - _inicioProcesso).TotalSeconds;

        var dto = new DiagnosticoRuntimeDto(
            SistemaOperacional: RuntimeInformation.OSDescription,
            Arquitetura: RuntimeInformation.OSArchitecture.ToString(),
            Framework: RuntimeInformation.FrameworkDescription,
            VersaoRuntime: Environment.Version.ToString(),
            Is64BitProcess: Environment.Is64BitProcess,
            TempoAtividadeSegundos: Math.Round(uptimeSeg, 1),
            MemoriaAlocadaMb: Math.Round(memoriaMb, 2),
            ModoExecucao: "Kestrel Cross-Platform / Container Ready"
        );

        return Ok(dto);
    }

    [HttpGet("comparativo")]
    public IActionResult ObterComparativo()
    {
        var comparativo = new List<ComparativoPublicacaoDto>
        {
            new ComparativoPublicacaoDto(
                Modo: "Framework-Dependent (FDD)",
                ComandoCli: "dotnet publish -c Release -o ./publish-fdd",
                TamanhoAproximado: "4 MB a 15 MB",
                TempoInicializacao: "< 120 ms",
                Portabilidade: "Requer runtime instalado no host de destino",
                Descricao: "Padrao mais comum para servidores de aplicacao corporativos com SDK/Runtime centralizado."
            ),
            new ComparativoPublicacaoDto(
                Modo: "Self-Contained (SCD)",
                ComandoCli: "dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish-scd",
                TamanhoAproximado: "70 MB a 95 MB",
                TempoInicializacao: "< 100 ms",
                Portabilidade: "Totalmente isolado; nao requer .NET instalado no host",
                Descricao: "Ideal para VMs Linux sem gestao centralizada de runtimes ou ambientes restritos."
            ),
            new ComparativoPublicacaoDto(
                Modo: "Single-File Executable",
                ComandoCli: "dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish-single",
                TamanhoAproximado: "65 MB a 85 MB (binario unico)",
                TempoInicializacao: "< 150 ms",
                Portabilidade: "Executavel unico auto-extraivel ou executado da memoria",
                Descricao: "Facilita distribuicao direta, ferramentas CLI e daemons sem pastas com centenas de DLLs."
            ),
            new ComparativoPublicacaoDto(
                Modo: "ReadyToRun (R2R)",
                ComandoCli: "dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true -o ./publish-r2r",
                TamanhoAproximado: "Aumento de 20% a 30% no tamanho",
                TempoInicializacao: "< 40 ms (tempo de JIT quase nulo)",
                Portabilidade: "Especifico para a arquitetura alvo (RID)",
                Descricao: "Pre-compila IL para codigo nativo, eliminando aquecimento no primeiro request HTTP."
            ),
            new ComparativoPublicacaoDto(
                Modo: "Container Chiseled (OCI)",
                ComandoCli: "docker build -t minha-app:latest .",
                TamanhoAproximado: "~90 MB (imagem completa)",
                TempoInicializacao: "< 200 ms no Kubernetes",
                Portabilidade: "Imutavel em qualquer host Linux/Kubernetes",
                Descricao: "Imagem sem gerenciador de pacotes e sem shell interativo, garantindo seguranca extrema."
            )
        };

        return Ok(comparativo);
    }

    [HttpPost("gerar-comando")]
    public IActionResult GerarComando([FromBody] GeradorComandoRequest req)
    {
        var partes = new List<string> { "dotnet publish" };

        partes.Add($"-c {req.Configuracao}");
        if (!string.IsNullOrWhiteSpace(req.Rid) && req.Rid != "portable")
        {
            partes.Add($"-r {req.Rid}");
        }

        if (req.SelfContained)
        {
            partes.Add("--self-contained true");
        }
        else
        {
            partes.Add("--self-contained false");
        }

        if (req.SingleFile)
        {
            partes.Add("-p:PublishSingleFile=true");
            if (req.EnableCompression)
            {
                partes.Add("-p:EnableCompressionInSingleFile=true");
            }
        }

        if (req.ReadyToRun)
        {
            partes.Add("-p:PublishReadyToRun=true");
        }

        partes.Add("-o ./dist");

        var comando = string.Join(" ", partes);
        var estrategia = req.SelfContained
            ? "Publicacao auto-suficiente: o binario contera o runtime .NET 10 embutido."
            : "Publicacao dependente de framework: o host de destino precisa do runtime instalado.";

        var artefato = req.SingleFile
            ? (req.Rid.StartsWith("win") ? "dist/PublishDeployDemo.exe (Arquivo unico)" : "dist/PublishDeployDemo (Binario unico)")
            : "dist/ (Pasta com assemblies, dependencias e arquivo executavel)";

        return Ok(new GeradorComandoResponse(comando, estrategia, artefato));
    }
}
