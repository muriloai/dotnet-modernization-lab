using System.Collections.Generic;

namespace EnvironmentDemo.Models
{
    public class FeatureLegadaItem
    {
        public string Chave { get; set; }
        public bool Ativa { get; set; }
        public string Descricao { get; set; }
    }

    public class EnvironmentLegadoDto
    {
        public string NomeAmbiente { get; set; }
        public string ArquivoFonte { get; set; }
        public List<FeatureLegadaItem> Features { get; set; }
    }
}
