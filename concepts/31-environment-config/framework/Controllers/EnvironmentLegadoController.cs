using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using EnvironmentDemo.Models;

namespace EnvironmentDemo.Controllers
{
    public class EnvironmentLegadoController : Controller
    {
        public ActionResult Index()
        {
            var model = ObterDados();
            return View(model);
        }

        [HttpGet]
        public ActionResult ObterStatus()
        {
            return Json(ObterDados(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProcessarCheckout()
        {
            // Leitura de string em AppSettings
            bool novoCheckout = ConfigurationManager.AppSettings["Feature_NovoCheckout"] == "true";
            bool desconto = ConfigurationManager.AppSettings["Feature_DescontoBlackFriday"] == "true";

            if (novoCheckout)
            {
                return Json(new
                {
                    mensagem = "Checkout processado pelo motor V2",
                    versao = "V2",
                    descontoAplicado = desconto
                });
            }

            return Json(new
            {
                mensagem = "Checkout processado pelo fluxo legado V1 (Boleto simples)",
                versao = "V1",
                descontoAplicado = false
            });
        }

        private EnvironmentLegadoDto ObterDados()
        {
            var flags = new List<FeatureLegadaItem>
            {
                new FeatureLegadaItem
                {
                    Chave = "Feature_NovoCheckout",
                    Ativa = ConfigurationManager.AppSettings["Feature_NovoCheckout"] == "true",
                    Descricao = "Chave estatica no Web.config"
                },
                new FeatureLegadaItem
                {
                    Chave = "Feature_DescontoBlackFriday",
                    Ativa = ConfigurationManager.AppSettings["Feature_DescontoBlackFriday"] == "true",
                    Descricao = "Chave estatica no Web.config"
                },
                new FeatureLegadaItem
                {
                    Chave = "Feature_ModoBetaRelatorios",
                    Ativa = ConfigurationManager.AppSettings["Feature_ModoBetaRelatorios"] == "true",
                    Descricao = "Chave estatica no Web.config"
                }
            };

            return new EnvironmentLegadoDto
            {
                NomeAmbiente = ConfigurationManager.AppSettings["Ambiente"] ?? "Padrao",
                ArquivoFonte = "Web.config (Monolitico)",
                Features = flags
            };
        }
    }
}
