using System;

namespace SolidArchitectureDemo.Models
{
    // Modelo anêmico legado: propriedades públicas sem encapsulamento nem validação de invariantes
    public class ClienteAcoplado
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public decimal SaldoCredito { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }

        public ClienteAcoplado()
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.Now;
            Ativo = true;
        }
    }
}
