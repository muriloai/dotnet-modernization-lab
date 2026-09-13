using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RoutingDemo.Models;

namespace RoutingDemo.Controllers.Api
{
    /// <summary>
    /// Controller do ASP.NET Web API 2 herdando de System.Web.Http.ApiController.
    /// Retorna dados em JSON utilizando a tabela de rotas separada do WebApiConfig.
    /// </summary>
    [RoutePrefix("api/produtos")]
    public class ProdutosApiController : ApiController
    {
        private static readonly List<Produto> Produtos = new List<Produto>
        {
            new Produto(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
            new Produto(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true),
            new Produto(3, "Teclado Mecânico RGB", "perifericos", 350.00m, false),
            new Produto(4, "Licença Visual Studio Professional", "software", 2500.00m, true)
        };

        [HttpGet]
        [Route("")]
        public IHttpActionResult ObterTodos()
        {
            return Ok(Produtos);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObterPorId(int id)
        {
            var produto = Produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }
    }
}
