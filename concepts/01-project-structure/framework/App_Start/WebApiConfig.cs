using System.Web.Http;

namespace ProjectStructure
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Nota Didática: No .NET Framework, o Web API 2 era uma stack completamente
            // separada do MVC 5, possuindo sua própria tabela de rotas e serializadores.
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
