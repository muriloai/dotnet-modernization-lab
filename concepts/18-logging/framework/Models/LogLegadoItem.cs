using System;

namespace LoggingDemo.Models
{
    public class LogLegadoItem
    {
        public DateTime DataHora { get; set; }
        public string Tipo { get; set; }
        public string Mensagem { get; set; }

        public LogLegadoItem()
        {
            DataHora = DateTime.Now;
            Tipo = "Informacao";
            Mensagem = string.Empty;
        }

        public LogLegadoItem(string tipo, string mensagem)
        {
            DataHora = DateTime.Now;
            Tipo = tipo;
            Mensagem = mensagem;
        }
    }
}
