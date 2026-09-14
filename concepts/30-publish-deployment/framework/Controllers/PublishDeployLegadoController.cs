using System;
using System.Configuration;
using System.Diagnostics;
using System.Web.Mvc;
using PublishDeployDemo.Models;

namespace PublishDeployDemo.Controllers
{
    public class PublishDeployLegadoController : Controller
    {
        public ActionResult Index()
        {
            var proc = Process.GetCurrentProcess();
            var info = new DeployLegadoInfoDto
            {
                SistemaOperacional = Environment.OSVersion.ToString(),
                VersaoClr = Environment.Version.ToString(),
                ServidorWeb = "IIS Express / W3WP.exe (Windows Only)",
                AmbienteConfigurado = ConfigurationManager.AppSettings["Ambiente"] ?? "Padrao",
                CaminhoDestinoIIS = ConfigurationManager.AppSettings["CaminhoIIS"] ?? @"C:\inetpub\wwwroot",
                IsGacPresente = true,
                MemoriaWorkingSetMb = Math.Round(proc.WorkingSet64 / (1024.0 * 1024.0), 2)
            };

            return View(info);
        }

        [HttpGet]
        public ActionResult ObterDadosJson()
        {
            var proc = Process.GetCurrentProcess();
            var info = new DeployLegadoInfoDto
            {
                SistemaOperacional = Environment.OSVersion.ToString(),
                VersaoClr = Environment.Version.ToString(),
                ServidorWeb = "IIS Express / W3WP.exe (Windows Only)",
                AmbienteConfigurado = ConfigurationManager.AppSettings["Ambiente"] ?? "Padrao",
                CaminhoDestinoIIS = ConfigurationManager.AppSettings["CaminhoIIS"] ?? @"C:\inetpub\wwwroot",
                IsGacPresente = true,
                MemoriaWorkingSetMb = Math.Round(proc.WorkingSet64 / (1024.0 * 1024.0), 2)
            };

            return Json(info, JsonRequestBehavior.AllowGet);
        }
    }
}
