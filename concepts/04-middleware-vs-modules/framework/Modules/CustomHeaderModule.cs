using System;
using System.Web;

namespace MiddlewareVsModulesDemo.Modules
{
    /// <summary>
    /// Módulo HTTP legado que adiciona cabeçalhos à resposta do IIS.
    /// Exige conectar-se ao evento PreSendRequestHeaders do HttpApplication.
    /// </summary>
    public class CustomHeaderModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            app.Response.Headers["X-Execution-Engine"] = "DotNet-Framework-4.8.1-IIS";
            app.Response.Headers["X-Pipeline-Type"] = "HttpModule-Events";
        }

        public void Dispose()
        {
        }
    }
}
