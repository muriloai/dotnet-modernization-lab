using System;

namespace AuthorizationPoliciesDemo.Models
{
    /// <summary>
    /// Entidade representativa de relatório para teste de regras no MVC clássico.
    /// </summary>
    public class RelatorioFinanceiro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public decimal Valor { get; set; }
        public string AutorEmail { get; set; }
        public string AutorNome { get; set; }
        public string Departamento { get; set; }
        public string Status { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
