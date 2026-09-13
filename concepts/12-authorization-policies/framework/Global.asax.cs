using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (ticket != null && !ticket.Expired)
                    {
                        var partes = (ticket.UserData ?? string.Empty).Split(';');

                        var usuarioSessao = new UsuarioSessao
                        {
                            Nome = ticket.Name,
                            Perfil = partes.Length > 0 ? partes[0] : "Operador",
                            Departamento = partes.Length > 1 ? partes[1] : "Geral",
                            Idade = partes.Length > 2 && int.TryParse(partes[2], out var idade) ? idade : 18,
                            AnosExperiencia = partes.Length > 3 && int.TryParse(partes[3], out var exp) ? exp : 0
                        };

                        var identity = new FormsIdentity(ticket);
                        var principal = new CustomPrincipal(identity, usuarioSessao);

                        HttpContext.Current.User = principal;
                        Thread.CurrentPrincipal = principal;
                    }
                }
                catch
                {
                    // Ticket inválido ou corrompido
                }
            }
        }
    }
}
