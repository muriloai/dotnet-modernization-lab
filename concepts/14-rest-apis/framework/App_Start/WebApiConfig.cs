using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace RestApisDemo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilita suporte ao roteamento por atributos do Web API 2
            config.MapHttpAttributeRoutes();

            // Rota convencional padrão da Web API 2
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // No .NET Framework 4.8.1, o ASP.NET Web API 2 utilizava o Newtonsoft.Json como formatador padrão
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
