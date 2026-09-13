using System;

namespace SecurityBestPracticesDemo.Models
{
    /// <summary>
    /// Modelo decorado com [Serializable] clássico.
    /// No .NET Framework 4.8.1, classes como esta eram serializadas com BinaryFormatter,
    /// abrindo brechas de Remote Code Execution (RCE) caso dados externos não confiáveis fossem desserializados.
    /// </summary>
    [Serializable]
    public class ObjetoSessaoLegado
    {
        public string IdentificadorSessao { get; set; }
        public string Usuario { get; set; }
        public decimal SaldoDisponivel { get; set; }
        public DateTime UltimoAcesso { get; set; }
    }
}
