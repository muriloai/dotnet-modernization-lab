using System.ComponentModel.DataAnnotations;

namespace AuthenticationDemo.Models;

/// <summary>
/// Modelo de requisição para autenticação.
/// </summary>
public sealed class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail deve ter um formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    public bool LembrarMe { get; set; }
}
