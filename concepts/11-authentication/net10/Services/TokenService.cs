using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationDemo.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationDemo.Services;

/// <summary>
/// Serviço de emissão de tokens JWT para demonstrar a interoperabilidade moderna entre Cookie e Bearer.
/// </summary>
public sealed class TokenService : ITokenService
{
    public const string ChaveSecreta = "ChaveSeguraDePeloMenos32BytesParaLaboratorio2026!";
    public const string Emissor = "DotNetModernizationLab";
    public const string Audiencia = "DotNetModernizationLab";
    public const int TempoExpiracaoMinutos = 60;

    public TokenResponse GerarToken(Usuario usuario)
    {
        var manipuladorToken = new JwtSecurityTokenHandler();
        var chave = Encoding.UTF8.GetBytes(ChaveSecreta);
        var dataExpiracao = DateTime.UtcNow.AddMinutes(TempoExpiracaoMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, usuario.Perfil),
            new("departamento", usuario.Departamento),
            new("tipo_emissao", "jwt_bearer_moderno")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = dataExpiracao,
            Issuer = Emissor,
            Audience = Audiencia,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(chave),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = manipuladorToken.CreateToken(tokenDescriptor);
        var tokenString = manipuladorToken.WriteToken(token);

        return new TokenResponse(
            Token: tokenString,
            TipoToken: "Bearer",
            ExpiraEmSegundos: TempoExpiracaoMinutos * 60,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Perfil: usuario.Perfil,
            DataExpiracaoUtc: dataExpiracao
        );
    }
}
