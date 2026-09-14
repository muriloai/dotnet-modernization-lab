using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeAotDemo.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AotJsonContext.Default);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var inicioProcesso = Process.GetCurrentProcess().StartTime.ToUniversalTime();

app.MapGet("/api/aot/diagnostico", () =>
{
    var proc = Process.GetCurrentProcess();
    var memoriaMb = proc.WorkingSet64 / (1024.0 * 1024.0);
    var uptime = (DateTime.UtcNow - inicioProcesso).TotalSeconds;
    bool isAot = !RuntimeFeature.IsDynamicCodeSupported;

    return TypedResults.Ok(new AotDiagnosticsDto(
        IsAotCompilado: isAot,
        ModoExecucao: isAot ? "Native AOT (Ahead-of-Time Compilado)" : "JIT Otimizado (.NET 10 Dev Mode)",
        MemoriaAlocadaMb: Math.Round(memoriaMb, 2),
        UptimeSegundos: Math.Round(uptime, 2),
        Plataforma: RuntimeInformation.OSDescription,
        VersaoRuntime: Environment.Version.ToString(),
        JitHabilitado: RuntimeFeature.IsDynamicCodeSupported,
        SerializadorUtilizado: "System.Text.Json Source Generator (Sem Reflection)"
    ));
});

app.MapGet("/api/aot/comparativo", () =>
{
    var comparativo = new List<AotComparativoDto>
    {
        new AotComparativoDto("Tempo de Cold Start (Inicializacao)", "1.500 ms a 4.000 ms", "8 ms a 18 ms", "Aproximadamente 99% mais rapido"),
        new AotComparativoDto("Consumo de Memoria Inicial (RAM)", "150 MB a 280 MB", "18 MB a 32 MB", "Reducao de ate 88%"),
        new AotComparativoDto("Dependencia do Compilador JIT", "Obrigatorio no processo", "Completamente removido", "Impossibilita geracao de codigo nao seguro"),
        new AotComparativoDto("Tamanho de Imagem Docker", "5 GB a 8 GB (Windows Core)", "25 MB a 40 MB (Chiseled Distroless)", "Reducao de mais de 99%"),
        new AotComparativoDto("Remocao de Codigo Morto (Trimming)", "Inexistente no legado", "Trimming estatico nativo", "Binario compacto sem assemblies orfaos"),
        new AotComparativoDto("Reflection em Runtime", "Intensa e generalizada", "Substituida por Source Generators", "Zero alocacao de metadados em runtime")
    };

    return TypedResults.Ok(comparativo);
});

app.MapPost("/api/aot/benchmark", () =>
{
    const int iteracoes = 100_000;
    long memoriaInicial = GC.GetTotalMemory(true);
    var sw = Stopwatch.StartNew();

    long soma = 0;
    for (int i = 0; i < iteracoes; i++)
    {
        soma += (i ^ (i << 2)) & 0xFFFFFF;
    }

    sw.Stop();
    long memoriaFinal = GC.GetTotalMemory(false);

    double tempoTotalUs = sw.Elapsed.TotalMicroseconds;
    double tempoPorOpNs = (sw.Elapsed.TotalNanoseconds) / iteracoes;

    return TypedResults.Ok(new BenchmarkResultadoDto(
        OperacoesExecutadas: iteracoes,
        TempoTotalMicrossegundos: Math.Round(tempoTotalUs, 2),
        TempoMedioPorOperacaoNanossegundos: Math.Round(tempoPorOpNs, 2),
        MemoriaFinalBytes: memoriaFinal
    ));
});

app.Run();
