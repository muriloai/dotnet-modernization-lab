using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace RoutingDemo
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();

            // PONTO CRITICO DE DESIGN NO .NET FRAMEWORK:
            // A ordem de chamada abaixo e fundamental. Se RouteConfig fosse registrado antes
            // de WebApiConfig, a rota padrao do MVC "{controller}/{action}/{id}" interceptaria
            // chamadas para /api/produtos tentando encontrar um ProdutosController MVC com uma action chamada "1",
            // resultando em erro 404 ou falha inesperada de roteamento.
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
