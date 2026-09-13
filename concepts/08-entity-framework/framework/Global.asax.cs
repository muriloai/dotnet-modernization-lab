using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using EntityFrameworkDemo.Data;

namespace EntityFrameworkDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Registro estático do inicializador do Entity Framework 6
            Database.SetInitializer(new DbInitializer());
        }
    }
}
