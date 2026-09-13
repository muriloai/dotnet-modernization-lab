namespace DependencyInjectionDemo.Services;

public record ClienteDto(int Id, string Nome, string Email, string Categoria);

public interface IClienteService
{
    IReadOnlyList<ClienteDto> ObterTodos();
    ClienteDto? ObterPorId(int id);
}
