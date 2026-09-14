using System;
using System.Collections.Generic;

namespace CacheLegadoDemo.Models
{
    public class ProdutoLegadoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
        public DateTime DataOrigem { get; set; }
    }

    public class CacheLegadoResultDto
    {
        public object Dados { get; set; }
        public string Origem { get; set; }
        public long TempoExecucaoMs { get; set; }
        public long ContadorConsultasBanco { get; set; }
        public string Mensagem { get; set; }
    }

    public class StampedeLegadoResultDto
    {
        public int TotalRequisicoes { get; set; }
        public int ExecucoesSemProtecao { get; set; }
        public long TempoTotalMs { get; set; }
        public string Diagnostico { get; set; }
    }
}
