using System.Collections.Generic;

namespace CachingDemo.Models
{
    public class MetricasStampede
    {
        public string Modo { get; set; } = string.Empty;
        public int RequisicoesDisparadas { get; set; }
        public int AcessosReaisAoBanco { get; set; }
        public double TempoTotalMs { get; set; }
        public int TotalItensRetornados { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}
