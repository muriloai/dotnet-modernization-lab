namespace HttpClientDemo.Services;

/// <summary>
/// Implementação em memória do simulador de falhas transientes.
/// Registrado como Singleton para rastrear retries entre chamadas HTTP.
/// </summary>
public sealed class SimuladorFalhasService : ISimuladorFalhasService
{
    private readonly Lock _lock = new();
    private int _tentativas;

    public bool SimularFalhaTransiente { get; set; } = false;
    public int FalhasConsecutivasParaSimular { get; set; } = 2;

    public int ContadorTentativas
    {
        get
        {
            lock (_lock)
            {
                return _tentativas;
            }
        }
    }

    public void RegistrarTentativa()
    {
        lock (_lock)
        {
            _tentativas++;
        }
    }

    public void Resetar()
    {
        lock (_lock)
        {
            _tentativas = 0;
            SimularFalhaTransiente = false;
        }
    }
}
