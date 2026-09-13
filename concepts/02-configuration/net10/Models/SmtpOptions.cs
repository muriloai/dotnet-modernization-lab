using System.ComponentModel.DataAnnotations;

namespace ConfigurationDemo.Models;

/// <summary>
/// Modelo tipado que representa a seção "Smtp" do appsettings.json.
/// No .NET Framework, isso exigia criar uma classe herdando de ConfigurationSection com dezenas de linhas.
/// No .NET 10, trata-se de uma classe POCO simples.
/// </summary>
public sealed class SmtpOptions
{
    public const string Secao = "Smtp";

    [Required(ErrorMessage = "O endereço do servidor SMTP é obrigatório.")]
    public string Servidor { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "A porta do servidor SMTP deve ser um número válido entre 1 e 65535.")]
    public int Porta { get; set; } = 587;

    public bool HabilitarSsl { get; set; } = true;

    [Required(ErrorMessage = "O e-mail do remetente é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail do remetente deve ter um formato de e-mail válido.")]
    public string EmailRemetente { get; set; } = string.Empty;
}
