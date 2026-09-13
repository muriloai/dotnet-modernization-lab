using System.Threading.Tasks;
using System.Web.Http;
using HttpClientDemo.Services;

namespace HttpClientDemo.Controllers.Api
{
    /// <summary>
    /// Controller da Web API 2 expondo endpoints para teste das abordagens legadas de clientes HTTP.
    /// </summary>
    [RoutePrefix("api/clientes-http")]
    public class ClientesHttpApiController : ApiController
    {
        [HttpGet]
        [Route("using/{moeda}")]
        public async Task<IHttpActionResult> TestarUsing(string moeda)
        {
            var resultado = await CotacaoLegadaService.ConsultarComUsingAsync(moeda);
            return Ok(resultado);
        }

        [HttpGet]
        [Route("static/{moeda}")]
        public async Task<IHttpActionResult> TestarStatic(string moeda)
        {
            var resultado = await CotacaoLegadaService.ConsultarComStaticAsync(moeda);
            return Ok(resultado);
        }

        [HttpGet]
        [Route("webclient/{moeda}")]
        public IHttpActionResult TestarWebClient(string moeda)
        {
            var resultado = CotacaoLegadaService.ConsultarComWebClient(moeda);
            return Ok(resultado);
        }
    }
}
