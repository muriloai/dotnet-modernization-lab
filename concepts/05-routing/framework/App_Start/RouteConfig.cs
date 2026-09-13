using System.Web.Mvc;
using System.Web.Routing;

namespace RoutingDemo
{
    /// <summary>
    /// Tabela de rotas exclusiva do ASP.NET MVC 5.
    /// Define convenções para renderização de views Razor.
    /// </summary>
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Rota customizada para detalhes de produtos
            routes.MapRoute(
                name: "ProdutoDetalhes",
                url: "produtos/detalhes/{id}",
                defaults: new { controller = "Produtos", action = "Detalhes" },
                constraints: new { id = @"\d+" } // Regex manual necessária para forçar inteiro no legado
            );

            // Rota padrão do MVC
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Produtos", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
