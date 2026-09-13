namespace AuthorizationPoliciesDemo.Models;

/// <summary>
/// Modelo de representação de usuário com seus atributos contextuais (Claims).
/// </summary>
public sealed record UsuarioInfo(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    string Departamento,
    int Idade,
    int AnosExperiencia,
    string Nivel
);
