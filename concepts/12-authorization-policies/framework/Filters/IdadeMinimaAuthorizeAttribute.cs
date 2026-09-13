using System;
using System.Web;
using System.Web.Mvc;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Filters
{
    /// <summary>
    /// Filtro customizado herdando de AuthorizeAttribute para validar idade mínima no MVC 5.
    /// Exibe a rigidez de ter a lógica de negócio amarrada ao atributo.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class IdadeMinimaAuthorizeAttribute : AuthorizeAttribute
    {
        public int IdadeMinima { get; }

        public IdadeMinimaAuthorizeAttribute(int idadeMinima)
        {
            IdadeMinima = idadeMinima;
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

            return principal.Dados.Idade >= IdadeMinima;
        }
    }
}
