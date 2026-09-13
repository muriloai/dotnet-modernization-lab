namespace AuthorizationPoliciesDemo.Models;

/// <summary>
/// Modelo de relatório financeiro utilizado para demonstrar autorização baseada em recursos (Resource-Based Authorization).
/// </summary>
public sealed class RelatorioFinanceiro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string AutorEmail { get; set; } = string.Empty;
    public string AutorNome { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
}
