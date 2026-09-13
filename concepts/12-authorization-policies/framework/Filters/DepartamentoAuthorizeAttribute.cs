using System;
using System.Web;
using System.Web.Mvc;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Filters
{
    /// <summary>
    /// Filtro de autorização customizado clássico do ASP.NET MVC 5.
    /// Demonstra a abordagem legada de criar classes de atributos derivadas de AuthorizeAttribute
    /// para regras que não fossem estritamente papéis (Roles).
    /// Note a ausência de injeção de dependência no construtor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class DepartamentoAuthorizeAttribute : AuthorizeAttribute
    {
        public string DepartamentoExigido { get; set; }

        public DepartamentoAuthorizeAttribute(string departamento)
        {
            DepartamentoExigido = departamento;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var autorizado = base.AuthorizeCore(httpContext);
            if (!autorizado)
            {
                return false;
            }

            var principal = httpContext.User as CustomPrincipal;
            if (principal == null || principal.Dados == null)
            {
                return false;
            }

            return principal.Dados.Departamento != null &&
                   principal.Dados.Departamento.Equals(DepartamentoExigido, StringComparison.OrdinalIgnoreCase);
        }
    }
}
