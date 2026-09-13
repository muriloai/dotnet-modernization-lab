namespace AuthenticationDemo.Models;

/// <summary>
/// Modelo de resposta com os dados do usuário autenticado e suas claims.
/// </summary>
public sealed record UsuarioResponse(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    string Departamento,
    string EsquemaAutenticacao,
    IReadOnlyDictionary<string, string> Claims
);
