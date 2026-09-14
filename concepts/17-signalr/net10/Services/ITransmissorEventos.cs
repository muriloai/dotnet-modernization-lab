using SignalRDemo.Models;

namespace SignalRDemo.Services;

/// <summary>
/// Contrato de serviço para disparo de eventos em tempo real a partir de fora do Hub.
/// </summary>
public interface ITransmissorEventos
{
    Task DispararNotificacaoExternaAsync(EnviarAvisoRequest request);
}
