using System.ComponentModel.DataAnnotations;

namespace ConfigurationDemo.Models;

/// <summary>
/// Modelo tipado que representa a seção "ConfiguracaoGeral" do appsettings.json.
/// Utiliza anotações de dados (Data Annotations) para validação declarativa.
/// </summary>
public sealed class ConfiguracaoGeralOptions
{
    public const string Secao = "ConfiguracaoGeral";

    [Required(ErrorMessage = "O nome do ambiente é obrigatório.")]
    public string Ambiente { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome do sistema é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome do sistema deve ter entre 3 e 100 caracteres.")]
    public string NomeSistema { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "O limite de itens por página deve estar entre 1 e 100.")]
    public int LimiteItensPorPagina { get; set; } = 20;

    public bool HabilitarAuditoria { get; set; } = false;
}
