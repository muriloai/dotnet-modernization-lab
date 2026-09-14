namespace LoggingDemo.Models;

public class LogItemVisualizacao
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string Nivel { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public Dictionary<string, object?> PropriedadesEstruturadas { get; set; } = new();
    public List<string> Escopos { get; set; } = new();
}
