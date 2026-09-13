using AuthenticationDemo.Models;

namespace AuthenticationDemo.Services;

/// <summary>
/// Contrato do serviço de usuários e autenticação de credenciais.
/// </summary>
public interface IUsuarioService
{
    Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
    Task<Usuario?> ObterPorIdAsync(int id);
    IEnumerable<Usuario> ListarUsuariosDemonstracao();
}
