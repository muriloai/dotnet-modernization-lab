using System.Web.Http;

namespace AuthenticationDemo.Controllers.Api
{
    /// <summary>
    /// API Controller demonstrando a leitura de identidade e autorização no Web API 2 clássico.
    /// </summary>
    [RoutePrefix("api/painel")]
    [Authorize]
    public class PainelApiController : ApiController
    {
        [HttpGet]
        [Route("meu-perfil")]
        public IHttpActionResult ObterMeuPerfil()
        {
            return Ok(new
            {
                Mensagem = "Acesso autorizado na Web API clássica!",
                Usuario = User.Identity.Name,
                TipoAutenticacao = User.Identity.AuthenticationType,
                EstaAutenticado = User.Identity.IsAuthenticated,
                IsAdministrador = User.IsInRole("Administrador"),
                IsOperador = User.IsInRole("Operador")
            });
        }

        [HttpGet]
        [Route("dados-restritos-admin")]
        [Authorize(Roles = "Administrador")]
        public IHttpActionResult ObterDadosRestritos()
        {
            return Ok(new
            {
                Status = "Sucesso",
                Mensagem = "Dados restritos acessados por Administrador na Web API 2.",
                ServidoresLegadosAtivos = 3
            });
        }
    }
}
