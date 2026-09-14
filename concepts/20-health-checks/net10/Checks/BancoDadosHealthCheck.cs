using HealthChecksDemo.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksDemo.Checks;

public class BancoDadosHealthCheck : IHealthCheck
{
    private readonly ISimuladorEstadoRecursos _simulador;

    public BancoDadosHealthCheck(ISimuladorEstadoRecursos simulador)
    {
        _simulador = simulador;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var latencia = _simulador.ObterLatenciaBanco();

        if (!_simulador.BancoDisponivel)
        {
            var dadosFalha = new Dictionary<string, object>
            {
                ["servidor"] = "sql-cluster-primario.interno",
                ["tempoTentativaMs"] = 3000,
                ["motivo"] = "Conexão recusada pelo host remoto"
            };

            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "O banco de dados relacional principal está inacessível.",
                    data: dadosFalha));
        }

        if (_simulador.BancoDegradado)
        {
            var dadosDegradados = new Dictionary<string, object>
            {
                ["latenciaMs"] = latencia.TotalMilliseconds,
                ["poolConexoes"] = "92% ocupado",
                ["alerta"] = "Tempo de resposta acima do limite estipulado de 500ms"
            };

            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Banco operacional, porém com latência elevada detectada: {latencia.TotalMilliseconds}ms.",
                    data: dadosDegradados));
        }

        var dadosNormais = new Dictionary<string, object>
        {
            ["latenciaMs"] = latencia.TotalMilliseconds,
            ["poolConexoes"] = "15% ocupado",
            ["statusReplica"] = "Sincronizada"
        };

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "Banco de dados operando normalmente com latência aceitável.",
                data: dadosNormais));
    }
}
