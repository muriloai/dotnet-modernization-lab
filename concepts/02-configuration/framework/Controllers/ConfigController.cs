using System;
using System.Configuration;
using System.Web.Mvc;
using ConfigurationDemo.Models;

namespace ConfigurationDemo.Controllers
{
    /// <summary>
    /// Controller responsável por demonstrar a leitura de configurações no .NET Framework.
    /// Utiliza a classe estática legada ConfigurationManager.
    /// </summary>
    public class ConfigController : Controller
    {
        public ActionResult Index()
        {
            // 1. Leitura de appSettings (valores retornam sempre como 'string' não tipada)
            string ambiente = ConfigurationManager.AppSettings["Ambiente"] ?? "Não configurado";
            string nomeSistema = ConfigurationManager.AppSettings["NomeSistema"] ?? "Sem nome";

            // Conversão manual necessária para tipos numéricos ou booleanos (suscetível a exceções de formato)
            int limiteItens = 10;
            if (int.TryParse(ConfigurationManager.AppSettings["LimiteItensPorPagina"], out int limiteLido))
            {
                limiteItens = limiteLido;
            }

            bool auditoria = false;
            if (bool.TryParse(ConfigurationManager.AppSettings["HabilitarAuditoria"], out bool auditoriaLida))
            {
                auditoria = auditoriaLida;
            }

            // 2. Leitura de ConnectionStrings
            string stringConexao = "Não configurada";
            var connConfig = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (connConfig != null)
            {
                stringConexao = connConfig.ConnectionString;
            }

            // 3. Leitura da seção customizada tipada
            var smtp = ConfigurationManager.GetSection("configuracaoSmtp") as ConfiguracaoSmtpSection;

            ViewBag.Ambiente = ambiente;
            ViewBag.NomeSistema = nomeSistema;
            ViewBag.LimiteItensPorPagina = limiteItens;
            ViewBag.HabilitarAuditoria = auditoria;
            ViewBag.StringConexao = stringConexao;
            ViewBag.Smtp = smtp;

            return View();
        }
    }
}
