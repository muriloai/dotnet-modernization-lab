using System;
using System.Net;
using System.Web.Http;
using RestApisDemo.Models;
using RestApisDemo.Services;

namespace RestApisDemo.Controllers.Api
{
    /// <summary>
    /// Controller RESTful clássico do ASP.NET Web API 2 (.NET Framework 4.8.1).
    /// Demonstra a herança obrigatória de ApiController, o uso de IHttpActionResult,
    /// a necessidade de validação manual de ModelState e respostas de erro sem padronização RFC 7807.
    /// </summary>
    [RoutePrefix("api/produtos")]
    public class ProdutosApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Listar(string categoria = null, bool? ativo = null)
        {
            var produtos = ProdutoRepositoryLegado.Listar(categoria, ativo);
            return Ok(produtos);
        }

        [HttpGet]
        [Route("{id:int}", Name = "ObterProdutoPorIdLegado")]
        public IHttpActionResult ObterPorId(int id)
        {
            var produto = ProdutoRepositoryLegado.ObterPorId(id);
            if (produto == null)
            {
                // No Web API 2, NotFound() retornava 404 sem corpo padronizado
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Criar(CriarProdutoModel model)
        {
            // No ASP.NET Web API 2, a checagem de validação era estritamente manual
            if (model == null)
            {
                return BadRequest("O corpo da requisição não pode ser nulo.");
            }

            if (!ModelState.IsValid)
            {
                // Retorna 400 Bad Request com estrutura de erro proprietária (HttpError)
                return BadRequest(ModelState);
            }

            if (ProdutoRepositoryLegado.ExisteSku(model.Sku))
            {
                // Código 409 Conflict implementado manualmente com payload anônimo ad-hoc
                return Content(HttpStatusCode.Conflict, new
                {
                    Erro = "Conflito de SKU",
                    Mensagem = string.Format("O SKU '{0}' já está cadastrado no sistema legado.", model.Sku)
                });
            }

            var criado = ProdutoRepositoryLegado.Adicionar(model);

            // Geração manual da URI de localização para o cabeçalho Location
            var locationUri = new Uri(Request.RequestUri + "/" + criado.Id);
            return Created(locationUri, criado);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Atualizar(int id, AtualizarProdutoModel model)
        {
            if (model == null)
            {
                return BadRequest("O corpo da requisição não pode ser nulo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var atualizado = ProdutoRepositoryLegado.Atualizar(id, model);
            if (atualizado == null)
            {
                return NotFound();
            }

            return Ok(atualizado);
        }

        [HttpPatch]
        [Route("{id:int}/preco")]
        public IHttpActionResult AtualizarPreco(int id, AtualizarPrecoModel model)
        {
            if (model == null)
            {
                return BadRequest("O corpo da requisição não pode ser nulo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var atualizado = ProdutoRepositoryLegado.AtualizarPreco(id, model.NovoPreco);
            if (atualizado == null)
            {
                return NotFound();
            }

            return Ok(atualizado);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Excluir(int id)
        {
            var removido = ProdutoRepositoryLegado.Excluir(id);
            if (!removido)
            {
                return NotFound();
            }

            // Retorna 204 No Content via StatusCode
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
