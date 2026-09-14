using HealthChecksDemo.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksDemo.Checks;

public class MemoriaHealthCheck : IHealthCheck
{
    private readonly ISimuladorEstadoRecursos _simulador;

    public MemoriaHealthCheck(ISimuladorEstadoRecursos simulador)
    {
        _simulador = simulador;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var memoriaAlocadaBytes = GC.GetTotalMemory(false);
        var memoriaAlocadaMb = memoriaAlocadaBytes / (1024.0 * 1024.0);
        var limiteMb = _simulador.LimiteMemoriaMegabytes;

        var dados = new Dictionary<string, object>
        {
            ["memoriaAlocadaMb"] = Math.Round(memoriaAlocadaMb, 2),
            ["limiteConfiguradoMb"] = limiteMb,
            ["geracaoColetor"] = GC.MaxGeneration
        };

        if (memoriaAlocadaMb > limiteMb)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    $"Consumo de memória ({memoriaAlocadaMb:F2} MB) excedeu o limite máximo estipulado de {limiteMb} MB.",
                    data: dados));
        }

        if (memoriaAlocadaMb > (limiteMb * 0.8))
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Consumo de memória ({memoriaAlocadaMb:F2} MB) está próximo do limiar crítico de {limiteMb} MB.",
                    data: dados));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                $"Consumo de memória gerenciada dentro dos parâmetros normais: {memoriaAlocadaMb:F2} MB.",
                data: dados));
    }
}
