using System;
using System.Collections.Generic;

namespace TraceLegadoDemo.Models
{
    public class PedidoLegadoRequest
    {
        public string Cliente { get; set; }
        public string Produto { get; set; }
        public decimal Valor { get; set; }
    }

    public class LogEntryLegado
    {
        public DateTime Timestamp { get; set; }
        public string Nivel { get; set; }
        public string Mensagem { get; set; }
        public string ThreadId { get; set; }
    }

    public class PedidoLegadoResponse
    {
        public string PedidoId { get; set; }
        public string CorrelationId { get; set; }
        public long TempoExecucaoMs { get; set; }
        public List<LogEntryLegado> LogsGerados { get; set; }
        public string MensagemDiagnostico { get; set; }
    }
}
