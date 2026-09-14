using HealthChecksDemo.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksDemo.Checks;

public class ServicoMensageriaHealthCheck : IHealthCheck
{
    private readonly ISimuladorEstadoRecursos _simulador;

    public ServicoMensageriaHealthCheck(ISimuladorEstadoRecursos simulador)
    {
        _simulador = simulador;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_simulador.MensageriaDisponivel)
        {
            var dadosFalha = new Dictionary<string, object>
            {
                ["broker"] = "rabbitmq-cluster.producao",
                ["porta"] = 5672,
                ["erro"] = "Broker inalcançável ou conexão de heartbeat expirada"
            };

            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "O cluster de mensageria externa está fora de operação.",
                    data: dadosFalha));
        }

        var dadosNormais = new Dictionary<string, object>
        {
            ["broker"] = "rabbitmq-cluster.producao",
            ["filasAtivas"] = 14,
            ["consumidoresConectados"] = 28
        };

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "Serviço de mensageria conectado e consumindo filas com sucesso.",
                data: dadosNormais));
    }
}
