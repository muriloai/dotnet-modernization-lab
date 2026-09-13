using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProjectStructure
{
    /// <summary>
    /// No ASP.NET Framework clássico, a classe Global herda de HttpApplication e
    /// orquestra o ciclo de vida da aplicação acoplada ao processo de trabalho do IIS (w3wp.exe).
    /// </summary>
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // No Framework, as configurações eram fragmentadas em métodos estáticos separados
            // em classes dentro da pasta App_Start/
            
            // 1. Configuração de rotas do ASP.NET Web API 2 (stack separada!)
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // 2. Configuração de filtros globais do MVC
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // 3. Configuração de rotas tradicionais do ASP.NET MVC 5
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Nota didática: Não existia container de Injeção de Dependências nativo.
            // Qualquer container (Autofac/Unity) precisava ser inicializado manualmente aqui.
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Evento fixo disparado no início de cada requisição HTTP pelo IIS
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            // Evento fixo disparado no término de cada requisição HTTP
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Captura rudimentar de exceções não tratadas no processo
            Exception ex = Server.GetLastError();
            // No Framework, exceptions não tratadas resultavam na famosa "Tela Amarela da Morte" (YSOD)
        }
    }
}
