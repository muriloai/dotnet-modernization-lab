using System.Web.Http;

namespace RoutingDemo
{
    /// <summary>
    /// Tabela de rotas SEPARADA para o ASP.NET Web API 2.
    /// Utiliza HttpConfiguration e HttpRouteCollection totalmente distintos do MVC.
    /// </summary>
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilita roteamento por atributos (Attribute Routing)
            config.MapHttpAttributeRoutes();

            // Rota de convenção padrão da Web API
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { id = @"\d*" } // Constraint manual de números
            );
        }
    }
}
