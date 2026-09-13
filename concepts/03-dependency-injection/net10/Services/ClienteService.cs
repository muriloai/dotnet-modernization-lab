namespace DependencyInjectionDemo.Services;

/// <summary>
/// Implementação do serviço de clientes utilizando Primary Constructor do C# moderno.
/// As dependências são declaradas diretamente nos parâmetros da classe, sem necessidade
/// de campos privados manuais nem atribuições redundantes.
/// </summary>
public sealed class ClienteService(ILogger<ClienteService> logger) : IClienteService
{
    private static readonly List<ClienteDto> Clientes =
    [
        new(1, "Ana Silva", "ana.silva@empresa.com.br", "Premium"),
        new(2, "Carlos Eduardo", "carlos.eduardo@empresa.com.br", "Padrão"),
        new(3, "Mariana Souza", "mariana.souza@empresa.com.br", "Premium")
    ];

    public IReadOnlyList<ClienteDto> ObterTodos()
    {
        logger.LogInformation("Consultando lista de clientes via ClienteService.");
        return Clientes.AsReadOnly();
    }

    public ClienteDto? ObterPorId(int id)
    {
        logger.LogInformation("Buscando cliente com Id {Id}.", id);
        return Clientes.FirstOrDefault(c => c.Id == id);
    }
}
