using System;

namespace MinimalApisDemo.Models
{
    public class TarefaLegada
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public bool Concluida { get; set; }
        public string Prioridade { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
