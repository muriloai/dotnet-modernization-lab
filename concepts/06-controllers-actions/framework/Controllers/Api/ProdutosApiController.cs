using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ControllersAndActionsDemo.Models;

namespace ControllersAndActionsDemo.Controllers.Api
{
    /// <summary>
    /// Controller do ASP.NET Web API 2 herdando de System.Web.Http.ApiController.
    /// Retorna respostas estruturadas com IHttpActionResult.
    /// </summary>
    [RoutePrefix("api/produtos")]
    public class ProdutosApiController : ApiController
    {
        private static readonly List<ProdutoDto> Produtos = new List<ProdutoDto>
        {
            new ProdutoDto(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
            new ProdutoDto(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true)
        };

        // GET: api/produtos
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObterTodos()
        {
            return Ok(Produtos);
        }

        // GET: api/produtos/1
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

        // POST: api/produtos
        // No Web API 2 clássico, ainda era obrigatório verificar manualmente o ModelState.IsValid:
        [HttpPost]
        [Route("")]
        public IHttpActionResult Criar([FromBody] ProdutoDto dto)
        {
            if (dto == null)
            {
                return BadRequest("O payload do produto não pode ser nulo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var novoId = Produtos.Count + 1;
            dto.Id = novoId;
            Produtos.Add(dto);

            return Created($"api/produtos/{novoId}", dto);
        }
    }
}
