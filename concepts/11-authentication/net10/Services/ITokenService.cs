using AuthenticationDemo.Models;

namespace AuthenticationDemo.Services;

/// <summary>
/// Contrato do serviço emissor de tokens JWT.
/// </summary>
public interface ITokenService
{
    TokenResponse GerarToken(Usuario usuario);
}
