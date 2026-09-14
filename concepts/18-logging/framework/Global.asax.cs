using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;

namespace LoggingDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Trace.TraceInformation("Aplicação iniciada no evento Application_Start via System.Diagnostics.Trace.");
        }
    }
}
