namespace HealthChecksDemo.Services;

public class SimuladorEstadoRecursos : ISimuladorEstadoRecursos
{
    public bool BancoDisponivel { get; set; } = true;
    public bool BancoDegradado { get; set; } = false;
    public bool MensageriaDisponivel { get; set; } = true;
    public long LimiteMemoriaMegabytes { get; set; } = 2048;

    public TimeSpan ObterLatenciaBanco()
    {
        if (BancoDegradado)
        {
            return TimeSpan.FromMilliseconds(750);
        }

        return TimeSpan.FromMilliseconds(25);
    }
}
