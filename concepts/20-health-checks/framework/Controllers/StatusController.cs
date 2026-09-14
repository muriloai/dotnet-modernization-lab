using System;
using System.Web.Mvc;
using HealthChecksDemo.Handlers;

namespace HealthChecksDemo.Controllers
{
    public class StatusController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (PingHandler.SimularFalhaBanco)
            {
                Response.StatusCode = 503;
                return Json(new
                {
                    status = "ERRO",
                    banco = false,
                    mensagem = "Falha ao executar SELECT 1 na base SQL legada"
                }, JsonRequestBehavior.AllowGet);
            }

            // Formato de resposta JSON proprietario do legado sem padronizacao
            return Json(new
            {
                status = "OK",
                banco = true,
                versao = "1.0.0",
                dataHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
