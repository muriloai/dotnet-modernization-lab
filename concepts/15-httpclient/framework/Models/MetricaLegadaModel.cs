namespace HttpClientDemo.Models
{
    /// <summary>
    /// Métricas de execução para diagnóstico das chamadas HTTP no legado.
    /// </summary>
    public class MetricaLegadaModel
    {
        public string ModoCliente { get; set; }
        public long TempoRespostaMs { get; set; }
        public int StatusCode { get; set; }
        public string Mensagem { get; set; }
        public CotacaoLegadaModel Dados { get; set; }
    }
}
