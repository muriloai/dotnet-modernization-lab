using System;
using System.Diagnostics;
using System.Web;

namespace MiddlewareVsModulesDemo.Modules
{
    /// <summary>
    /// Módulo HTTP legado implementando IHttpModule.
    /// No .NET Framework, para medir o tempo de uma requisição, era obrigatório conectar-se
    /// a dois eventos separados do HttpApplication (BeginRequest e EndRequest) e trafegar
    /// o estado manualmente através do dicionário estático HttpContext.Current.Items.
    /// </summary>
    public class RequestTimingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var stopwatch = Stopwatch.StartNew();
            app.Context.Items["RequestTimingStopwatch"] = stopwatch;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            if (app.Context.Items["RequestTimingStopwatch"] is Stopwatch stopwatch)
            {
                stopwatch.Stop();
                app.Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();
            }
        }

        public void Dispose()
        {
            // Limpeza de recursos não gerenciados, se necessário
        }
    }
}
