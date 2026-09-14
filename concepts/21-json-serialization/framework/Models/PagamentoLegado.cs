using System;

namespace JsonSerializationDemo.Models
{
    public class PagamentoLegado
    {
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; }
        public string ChavePix { get; set; }
        public string NumeroCartao { get; set; }
        public int Parcelas { get; set; }

        public PagamentoLegado()
        {
            DataHora = DateTime.Now;
        }
    }
}
