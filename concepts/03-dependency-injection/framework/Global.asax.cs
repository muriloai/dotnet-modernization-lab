using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DependencyInjectionDemo
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // No ASP.NET MVC 5, a injeção de dependências em controllers dependia
            // da definição explícita de um IDependencyResolver global:
            DependencyResolver.SetResolver(new CustomDependencyResolver());
        }
    }
}
