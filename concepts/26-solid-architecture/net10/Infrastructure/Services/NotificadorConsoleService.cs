using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolidArchitectureDemo.Application.Interfaces;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Infrastructure.Services;

public class NotificadorConsoleService(ILogger<NotificadorConsoleService> logger) : INotificadorService
{
    public Task EnviarBoasVindasAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Notificação enviada com sucesso para {Email} (Crédito Inicial: R$ {Saldo:F2})", cliente.Email, cliente.SaldoCredito);
        return Task.CompletedTask;
    }
}
