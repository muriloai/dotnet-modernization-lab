using System;

namespace HttpClientDemo.Models
{
    /// <summary>
    /// Modelo de cotação retornado no ecossistema .NET Framework 4.8.1.
    /// </summary>
    public class CotacaoLegadaModel
    {
        public string Moeda { get; set; }
        public string Nome { get; set; }
        public decimal ValorReais { get; set; }
        public decimal VariacaoPercentual { get; set; }
        public DateTime DataHora { get; set; }
        public string Fonte { get; set; }
    }
}
