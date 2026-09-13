using System.Web.Http;

namespace AuthorizationPoliciesDemo.Controllers.Api
{
    /// <summary>
    /// Web API 2 controller para demonstração de autorização clássica por endpoint REST.
    /// </summary>
    [RoutePrefix("api/documentos")]
    [Authorize]
    public class DocumentosApiController : ApiController
    {
        [HttpGet]
        [Route("painel")]
        public IHttpActionResult ObterPainel()
        {
            return Ok(new
            {
                Status = "Sucesso",
                Mensagem = "Acesso concedido via Web API clássica!",
                Usuario = User.Identity.Name
            });
        }

        [HttpGet]
        [Route("lideranca")]
        [Authorize(Roles = "Administrador,Gerente")]
        public IHttpActionResult ObterLideranca()
        {
            return Ok(new
            {
                Status = "Sucesso",
                Mensagem = "Área de liderança da Web API acessada com sucesso.",
                Usuario = User.Identity.Name
            });
        }
    }
}
