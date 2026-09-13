using AuthenticationDemo.Models;

namespace AuthenticationDemo.Services;

/// <summary>
/// Implementação do serviço de validação de usuários para o laboratório didático.
/// </summary>
public sealed class UsuarioService : IUsuarioService
{
    private static readonly List<Usuario> Usuarios = new()
    {
        new Usuario(1, "Ana Administradora", "admin@empresa.com", "admin123", "Administrador", "Tecnologia da Informação"),
        new Usuario(2, "Carlos Operador", "operador@empresa.com", "operador123", "Operador", "Atendimento ao Cliente")
    };

    public Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
    {
        var usuario = Usuarios.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            u.Senha == senha);

        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObterPorIdAsync(int id)
    {
        var usuario = Usuarios.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(usuario);
    }

    public IEnumerable<Usuario> ListarUsuariosDemonstracao() => Usuarios;
}
