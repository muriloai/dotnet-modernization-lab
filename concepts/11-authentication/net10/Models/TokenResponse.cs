namespace AuthenticationDemo.Models;

/// <summary>
/// Modelo de resposta contendo o token JWT gerado e metadados de expiração.
/// </summary>
public sealed record TokenResponse(
    string Token,
    string TipoToken,
    int ExpiraEmSegundos,
    string Nome,
    string Email,
    string Perfil,
    DateTime DataExpiracaoUtc
);
