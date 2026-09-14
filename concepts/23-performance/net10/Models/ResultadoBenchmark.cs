namespace PerformanceDemo.Models
{
    public class ResultadoBenchmark
    {
        public string Metodo { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public int ItensProcessados { get; set; }
        public double TempoMs { get; set; }
        public long BytesAlocadosEstimados { get; set; }
        public double MegabytesAlocados => System.Math.Round((double)BytesAlocadosEstimados / (1024.0 * 1024.0), 2);
        public int ColetasGen0 { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}
