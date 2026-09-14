using System;

namespace BackgroundServicesDemo.Models
{
    public class TarefaSegundoPlanoLegada
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public DateTime CriadoEm { get; set; }
        public string Status { get; set; }
        public int ProgressoPercentual { get; set; }
        public string Resultado { get; set; }

        public TarefaSegundoPlanoLegada()
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.Now;
            Status = "Pendente";
            ProgressoPercentual = 0;
        }
    }
}
