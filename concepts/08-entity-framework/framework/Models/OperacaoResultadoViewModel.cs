namespace EntityFrameworkDemo.Models
{
    public class OperacaoResultadoViewModel
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string ComandoSqlGerado { get; set; }
        public int RegistrosAfetados { get; set; }
        public long TempoExecucaoMs { get; set; }
        public bool Sucesso { get; set; }

        public OperacaoResultadoViewModel()
        {
            Sucesso = true;
        }
    }
}
