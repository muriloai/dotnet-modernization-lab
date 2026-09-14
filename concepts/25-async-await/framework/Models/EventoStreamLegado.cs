using System;

namespace AsyncAwaitDemo.Models
{
    public class EventoStreamLegado
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public DateTime Timestamp { get; set; }
        public int LatenciaMs { get; set; }

        public EventoStreamLegado()
        {
        }

        public EventoStreamLegado(int id, string descricao, DateTime timestamp, int latenciaMs)
        {
            Id = id;
            Descricao = descricao;
            Timestamp = timestamp;
            LatenciaMs = latenciaMs;
        }
    }
}
