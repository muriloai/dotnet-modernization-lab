using System.Text.Json.Serialization;

namespace NativeAotDemo.Models;

public record AotDiagnosticsDto(
    bool IsAotCompilado,
    string ModoExecucao,
    double MemoriaAlocadaMb,
    double UptimeSegundos,
    string Plataforma,
    string VersaoRuntime,
    bool JitHabilitado,
    string SerializadorUtilizado
);

public record AotComparativoDto(
    string Metrica,
    string NetFramework481Jit,
    string Net10NativeAot,
    string GanhoEficiencia
);

public record BenchmarkResultadoDto(
    int OperacoesExecutadas,
    double TempoTotalMicrossegundos,
    double TempoMedioPorOperacaoNanossegundos,
    long MemoriaFinalBytes
);

[JsonSerializable(typeof(AotDiagnosticsDto))]
[JsonSerializable(typeof(List<AotComparativoDto>))]
[JsonSerializable(typeof(BenchmarkResultadoDto))]
public partial class AotJsonContext : JsonSerializerContext
{
}
