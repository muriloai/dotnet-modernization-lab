using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SecurityBestPracticesDemo.Filters;
using SecurityBestPracticesDemo.Models;

namespace SecurityBestPracticesDemo.Controllers.Api
{
    /// <summary>
    /// Web API 2 controller demonstrando o uso do filtro customizado [ValidarAntiCsrfWebApi].
    /// </summary>
    [RoutePrefix("api/operacoes")]
    public class OperacoesApiController : ApiController
    {
        [HttpPost]
        [Route("transferir-protegida")]
        [ValidarAntiCsrfWebApi]
        public IHttpActionResult TransferirProtegida([FromBody] TransferenciaModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new
            {
                Status = "Sucesso",
                Mensagem = "Transferência via Web API 2 autorizada pelo filtro manual ValidarAntiCsrfWebApi!",
                Valor = model.Valor,
                ContaOrigem = model.ContaOrigem,
                ContaDestino = model.ContaDestino
            });
        }

        [HttpGet]
        [Route("headers-ativos")]
        public IHttpActionResult ObterHeaders()
        {
            return Ok(new
            {
                Mensagem = "Headers de segurança estáticos configurados no Web.config legado:",
                OrigemCORS = "Access-Control-Allow-Origin: * (Liberação global arriscada no XML)",
                XFrameOptions = "X-Frame-Options: SAMEORIGIN",
                XContentTypeOptions = "X-Content-Type-Options: nosniff"
            });
        }
    }
}
