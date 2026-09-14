namespace HealthChecksDemo.Services;

public interface ISimuladorEstadoRecursos
{
    bool BancoDisponivel { get; set; }
    bool BancoDegradado { get; set; }
    bool MensageriaDisponivel { get; set; }
    long LimiteMemoriaMegabytes { get; set; }

    TimeSpan ObterLatenciaBanco();
}
