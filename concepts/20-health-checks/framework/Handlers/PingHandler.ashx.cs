using System;
using System.Web;

namespace HealthChecksDemo.Handlers
{
    public class PingHandler : IHttpHandler
    {
        public static bool SimularFalhaBanco { get; set; } = false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            if (SimularFalhaBanco)
            {
                // No legado, falha de dependencia derrubava o endpoint inteiro de ping
                context.Response.StatusCode = 500;
                context.Response.Write("ERRO: Falha na conexão com o banco de dados legado (SELECT 1 falhou).");
                return;
            }

            // Retorno simplista de texto plano sem metadados
            context.Response.StatusCode = 200;
            context.Response.Write("OK");
        }

        public bool IsReusable => true;
    }
}
