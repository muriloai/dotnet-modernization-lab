using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SecurityBestPracticesDemo.Filters
{
    /// <summary>
    /// Filtro manual para validação de CSRF no ASP.NET Web API 2.
    /// Demonstra a fragmentação do .NET Framework clássico, onde o atributo [ValidateAntiForgeryToken]
    /// do MVC não funcionava em controllers de Web API, exigindo implementação customizada.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidarAntiCsrfWebApiAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-XSRF-TOKEN";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                var headers = actionContext.Request.Headers;
                string cookieToken = string.Empty;
                string formToken = string.Empty;

                // Extrai o token do cookie
                var cookieHeader = headers.GetCookies().FirstOrDefault();
                if (cookieHeader != null)
                {
                    var tokenCookie = cookieHeader[AntiForgeryConfig.CookieName];
                    if (tokenCookie != null)
                    {
                        cookieToken = tokenCookie.Value;
                    }
                }

                // Extrai o token do cabeçalho customizado da requisição
                if (headers.Contains(HeaderName))
                {
                    formToken = headers.GetValues(HeaderName).FirstOrDefault();
                }

                // Validação manual usando a classe utilitária AntiForgery do System.Web.Helpers
                AntiForgery.Validate(cookieToken, formToken);
            }
            catch (Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Erro = "Falha de validação Anti-CSRF na Web API clássica.",
                        Detalhe = ex.Message
                    }
                );
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
