using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;

namespace ErrorHandlingDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ultimaExcecao = Server.GetLastError();

            if (ultimaExcecao != null)
            {
                Trace.TraceError("Falha não tratada capturada no evento Application_Error: " + ultimaExcecao.ToString());

                // No legado, era comum armazenar o erro em Context.Items ou Session antes de redirecionar
                Context.Items["UltimaExcecao"] = ultimaExcecao;
            }
        }
    }
}
