namespace BackgroundServicesDemo.Models;

public record TarefaSegundoPlano
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Descricao { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
    public DateTime? IniciadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public string Status { get; set; } = "Pendente";
    public int ProgressoPercentual { get; set; }
    public string? Resultado { get; set; }
}

public record CriarTarefaRequest(string Descricao);

public record MetricasWorkerResponse(
    int TotalEnfileiradas,
    int TotalConcluidas,
    int TotalEmProcessamento,
    int TotalTicksTimer,
    DateTime UltimoTickTimer,
    long MemoriaAlocadaBytes
);
