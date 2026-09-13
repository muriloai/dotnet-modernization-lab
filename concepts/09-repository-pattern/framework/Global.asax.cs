using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using RepositoryPatternDemo.Data;

namespace RepositoryPatternDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Registro do inicializador do Entity Framework 6
            Database.SetInitializer(new DbInitializer());
        }
    }
}
