using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace AuthenticationDemo
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Evento do ciclo de vida do IIS disparado após o módulo de autenticação processar a requisição.
        /// No modelo clássico, o desenvolvedor interceptava este ponto para decodificar o FormsAuthenticationTicket
        /// e atribuir um GenericPrincipal com os papéis do usuário ao HttpContext.Current.User.
        /// </summary>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    // Decriptografia manual acoplada ao MachineKey configurado
                    var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (ticket != null && !ticket.Expired)
                    {
                        var userData = ticket.UserData ?? string.Empty;
                        var partes = userData.Split(';');
                        var perfil = partes.Length > 0 ? partes[0] : "Operador";

                        var identity = new FormsIdentity(ticket);
                        var roles = string.IsNullOrEmpty(perfil) ? new string[0] : new[] { perfil };
                        var principal = new GenericPrincipal(identity, roles);

                        // Atribui ao contexto da thread e da requisição
                        HttpContext.Current.User = principal;
                        Thread.CurrentPrincipal = principal;
                    }
                }
                catch
                {
                    // Caso o MachineKey seja incompatível ou o cookie esteja corrompido
                }
            }
        }
    }
}
