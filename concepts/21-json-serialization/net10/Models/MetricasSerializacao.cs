namespace JsonSerializationDemo.Models;

public class MetricasSerializacao
{
    public int TotalItens { get; set; }
    public double TempoReflexaoMs { get; set; }
    public double TempoSourceGenMs { get; set; }
    public double GanhoPercentualTempo { get; set; }
    public long MemoriaAlocadaReflexaoBytes { get; set; }
    public long MemoriaAlocadaSourceGenBytes { get; set; }
    public double ReducaoMemoriaPercentual { get; set; }
}
