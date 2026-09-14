using System.Threading;
using System.Threading.Tasks;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Application.Interfaces;

public interface INotificadorService
{
    Task EnviarBoasVindasAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
