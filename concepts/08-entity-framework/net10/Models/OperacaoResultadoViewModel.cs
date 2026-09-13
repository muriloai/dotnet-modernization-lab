namespace EntityFrameworkDemo.Models;

public class OperacaoResultadoViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ComandoSqlGerado { get; set; } = string.Empty;
    public int RegistrosAfetados { get; set; }
    public long TempoExecucaoMs { get; set; }
    public bool Sucesso { get; set; } = true;
}
