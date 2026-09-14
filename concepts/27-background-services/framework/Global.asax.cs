using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BackgroundServicesDemo.Services;

namespace BackgroundServicesDemo
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // No legado, era comum iniciar timers soltos no Application_Start
            FilaProcessamentoLegada.IniciarTimerMonitoramento();
        }

        void Application_End(object sender, EventArgs e)
        {
            FilaProcessamentoLegada.PararTimerMonitoramento();
        }
    }
}
