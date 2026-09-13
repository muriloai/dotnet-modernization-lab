using System;
using System.Web;

namespace MiddlewareVsModulesDemo.Handlers
{
    /// <summary>
    /// Handler HTTP legado implementando IHttpHandler.
    /// No .NET Framework, respostas simples ou endpoints diretos que não precisavam
    /// do mecanismo de controllers e views do MVC eram construídos com arquivos .ashx.
    /// </summary>
    public class HealthCheckHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write("{\"status\":\"Saudavel\",\"servidor\":\"Windows-IIS\",\"tecnologia\":\"IHttpHandler (.ashx)\"}");
        }

        public bool IsReusable => true;
    }
}
