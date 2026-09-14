using System;

namespace PublishDeployDemo.Models
{
    public class DeployLegadoInfoDto
    {
        public string SistemaOperacional { get; set; }
        public string VersaoClr { get; set; }
        public string ServidorWeb { get; set; }
        public string AmbienteConfigurado { get; set; }
        public string CaminhoDestinoIIS { get; set; }
        public bool IsGacPresente { get; set; }
        public double MemoriaWorkingSetMb { get; set; }
    }
}
