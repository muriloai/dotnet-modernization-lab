namespace HttpClientDemo.Services;

/// <summary>
/// Contrato do simulador de falhas para testes de resiliência e retries.
/// </summary>
public interface ISimuladorFalhasService
{
    bool SimularFalhaTransiente { get; set; }
    int FalhasConsecutivasParaSimular { get; set; }
    int ContadorTentativas { get; }
    void RegistrarTentativa();
    void Resetar();
}
