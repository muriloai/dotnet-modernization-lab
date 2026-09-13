using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataValidationDemo.Models;

namespace DataValidationDemo.Controllers.Api
{
    /// <summary>
    /// Controller da Web API 2 clássica do .NET Framework 4.8.1.
    /// Demonstra a necessidade de verificações manuais de ModelState.IsValid e o formato de erro proprietário.
    /// </summary>
    [RoutePrefix("api/clientes")]
    public class ClientesApiController : ApiController
    {
        private static readonly ConcurrentDictionary<int, ClienteDto> ClientesDb = new ConcurrentDictionary<int, ClienteDto>();
        private static int _proximoId = 1;

        static ClientesApiController()
        {
            ClientesDb[1] = new ClienteDto
            {
                Nome = "Maria Silva",
                Email = "maria.silva@exemplo.com.br",
                Cpf = "11144477735",
                Idade = 34,
                RendaMensal = 8500.00m,
                LimiteCreditoSolicitado = 25000.00m,
                PossuiRepresentante = false
            };
            _proximoId = 2;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Listar()
        {
            return Ok(ClientesDb.Values);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ObterPorId(int id)
        {
            ClienteDto cliente;
            if (ClientesDb.TryGetValue(id, out cliente))
            {
                return Ok(cliente);
            }

            // No modelo clássico não havia ProblemDetails por padrão.
            // Retornava-se NotFound com texto simples ou objeto anônimo.
            return Content(HttpStatusCode.NotFound, new
            {
                Mensagem = string.Format("Nenhum cliente foi localizado com o identificador {0}.", id)
            });
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Criar([FromBody] ClienteDto dto)
        {
            // 1. No .NET Framework, se o corpo estiver corrompido ou vazio, o parâmetro é null
            if (dto == null)
            {
                return BadRequest("O corpo da requisição não pode ser nulo.");
            }

            // 2. No .NET Framework, a validação NÃO bloqueia a execução automaticamente!
            // O desenvolvedor é obrigado a lembrar de verificar ModelState.IsValid em toda action.
            if (!ModelState.IsValid)
            {
                // BadRequest(ModelState) devolve o formato proprietário:
                // { "message": "The request is invalid.", "modelState": { "dto.Campo": ["Erro"] } }
                return BadRequest(ModelState);
            }

            // 3. Validação de regra de negócio (sem padrão RFC 7807 nativo)
            if (dto.Email != null && dto.Email.EndsWith("@bloqueado.com", StringComparison.OrdinalIgnoreCase))
            {
                return Content(HttpStatusCode.Conflict, new
                {
                    Mensagem = "O domínio de e-mail informado (@bloqueado.com) está em lista de restrição.",
                    CodigoErro = "EMAIL_BLOQUEADO"
                });
            }

            int id = System.Threading.Interlocked.Increment(ref _proximoId);
            ClientesDb[id] = dto;

            return Created(Request.RequestUri + "/" + id, new
            {
                Id = id,
                Mensagem = "Cliente cadastrado com sucesso via Web API clássica!",
                Dados = dto
            });
        }

        [HttpPost]
        [Route("simular-falha-interna")]
        public IHttpActionResult SimularFalhaInterna()
        {
            // Na Web API clássica, exceções geram mensagens em formato genérico:
            // { "message": "An error has occurred." } ou página HTML de erro.
            throw new InvalidOperationException("Falha simulada na integração com o bureau de crédito.");
        }
    }
}
