namespace PerformanceDemo.Models
{
    public class ResultadoBenchmarkLegado
    {
        public string Metodo { get; set; }
        public int TotalProcessado { get; set; }
        public double TempoMs { get; set; }
        public long MemoriaEstimadaBytes { get; set; }
        public double MemoriaEstimadaMb => System.Math.Round((double)MemoriaEstimadaBytes / (1024.0 * 1024.0), 2);
        public int ColetasGen0 { get; set; }
    }
}
