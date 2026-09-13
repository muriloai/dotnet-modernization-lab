using System;
using System.Security.Principal;

namespace AuthorizationPoliciesDemo.Models
{
    /// <summary>
    /// Dados contextuais do usuário no modelo clássico.
    /// </summary>
    public class UsuarioSessao
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }
        public string Departamento { get; set; }
        public int Idade { get; set; }
        public int AnosExperiencia { get; set; }
    }

    /// <summary>
    /// Implementação customizada de IPrincipal necessária no .NET Framework 4.8.1
    /// para carregar propriedades customizadas que não existiam no GenericPrincipal padrão.
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; }
        public UsuarioSessao Dados { get; }

        public CustomPrincipal(IIdentity identity, UsuarioSessao dados)
        {
            Identity = identity;
            Dados = dados;
        }

        public bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role) || Dados == null)
            {
                return false;
            }

            var perfisAceitos = role.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in perfisAceitos)
            {
                if (Dados.Perfil != null && Dados.Perfil.Trim().Equals(p.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
