namespace AuthenticationDemo.Models;

/// <summary>
/// Representa a entidade de usuário no sistema.
/// </summary>
public sealed record Usuario(
    int Id,
    string Nome,
    string Email,
    string Senha,
    string Perfil,
    string Departamento
);
