using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ActionFiltersDemo.Services;

namespace ActionFiltersDemo.Filters
{
    /// <summary>
    /// Filtro da Web API 2 demonstrando a limitação de DI no .NET Framework.
    /// Como atributos não suportavam injeção via construtor, o código precisava recorrer
    /// a validadores estáticos ou Service Locator.
    /// Para realizar curto-circuito, atribui-se uma HttpResponseMessage em actionContext.Response.
    /// </summary>
    public class ValidarApiKeyLegadoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.TryGetValues("X-API-Key", out var valores))
            {
                if (ValidadorApiKeyEstatico.Validar(valores))
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
            }

            // Curto-circuito na Web API 2 legada: interrompe a requisição atribuindo a resposta diretamente
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                Status = 401,
                Erro = "Acesso Negado pelo Filtro Legado",
                Mensagem = "Cabeçalho 'X-API-Key' ausente ou inválido no ASP.NET Web API 2.",
                ChaveEsperada = ValidadorApiKeyEstatico.ChavePadraoEsperada,
                Filtro = nameof(ValidarApiKeyLegadoAttribute)
            });
        }
    }
}
