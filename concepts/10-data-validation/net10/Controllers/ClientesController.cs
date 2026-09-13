using System.Collections.Concurrent;
using DataValidationDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataValidationDemo.Controllers;

/// <summary>
/// Controller moderno de clientes demonstrando o comportamento do [ApiController] e a padronização ProblemDetails (RFC 7807).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json", "application/problem+json")]
public sealed class ClientesController : ControllerBase
{
    // Simulação de base de dados em memória para testes interativos
    private static readonly ConcurrentDictionary<int, ClienteResponse> ClientesDb = new();
    private static int _proximoId = 1;

    static ClientesController()
    {
        // Registro inicial para testes
        var inicial = new ClienteResponse(
            Id: _proximoId++,
            Nome: "Maria Silva",
            Email: "maria.silva@exemplo.com.br",
            Cpf: "11144477735",
            Idade: 34,
            RendaMensal: 8500.00m,
            LimiteCreditoAprovado: 25000.00m,
            PossuiRepresentante: false,
            NomeRepresentante: null,
            DataCadastro: DateTime.UtcNow
        );
        ClientesDb[inicial.Id] = inicial;
    }

    /// <summary>
    /// Lista todos os clientes cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ClienteResponse>> Listar()
    {
        return Ok(ClientesDb.Values.OrderByDescending(c => c.Id));
    }

    /// <summary>
    /// Obtém um cliente pelo identificador. Retorna ProblemDetails 404 caso não exista.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ClienteResponse> ObterPorId(int id)
    {
        if (ClientesDb.TryGetValue(id, out var cliente))
        {
            return Ok(cliente);
        }

        // No .NET 10, erros HTTP são gerados com a estrutura RFC 7807 via método Problem()
        return Problem(
            detail: $"Nenhum cliente foi localizado com o identificador {id}.",
            statusCode: StatusCodes.Status404NotFound,
            title: "Cliente Não Encontrado",
            instance: HttpContext.Request.Path
        );
    }

    /// <summary>
    /// Cadastra um novo cliente.
    /// O atributo [ApiController] intercepta requisições inválidas automaticamente antes de executar este método,
    /// retornando 400 Bad Request com HttpValidationProblemDetails.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<ClienteResponse> Criar([FromBody] ClienteRequest request)
    {
        // Note que não há necessidade de:
        // 1. if (request == null) return BadRequest(); -> o framework rejeita corpo nulo ou mal formatado.
        // 2. if (!ModelState.IsValid) return BadRequest(ModelState); -> o [ApiController] executa o filtro automaticamente.

        // Demonstração de regra de negócio retornando ProblemDetails padronizado (409 Conflict)
        if (request.Email.EndsWith("@bloqueado.com", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                detail: "O domínio de e-mail informado (@bloqueado.com) está em lista de restrição de segurança.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito de Regra de Negócio",
                instance: HttpContext.Request.Path
            );
        }

        // Verifica unicidade de e-mail
        if (ClientesDb.Values.Any(c => c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Problem(
                detail: $"Já existe um cliente cadastrado com o e-mail '{request.Email}'.",
                statusCode: StatusCodes.Status409Conflict,
                title: "E-mail Já Cadastrado",
                instance: HttpContext.Request.Path
            );
        }

        var id = Interlocked.Increment(ref _proximoId);
        var novoCliente = new ClienteResponse(
            Id: id,
            Nome: request.Nome.Trim(),
            Email: request.Email.Trim().ToLowerInvariant(),
            Cpf: request.Cpf.Trim(),
            Idade: request.Idade,
            RendaMensal: request.RendaMensal,
            LimiteCreditoAprovado: request.LimiteCreditoSolicitado,
            PossuiRepresentante: request.PossuiRepresentante,
            NomeRepresentante: request.PossuiRepresentante ? request.NomeRepresentante?.Trim() : null,
            DataCadastro: DateTime.UtcNow
        );

        ClientesDb[novoCliente.Id] = novoCliente;

        return CreatedAtAction(nameof(ObterPorId), new { id = novoCliente.Id }, novoCliente);
    }

    /// <summary>
    /// Endpoint didático para demonstrar como exceções não tratadas são capturadas
    /// pelo middleware ProblemDetails do .NET 10, gerando resposta RFC 7807 com status 500.
    /// </summary>
    [HttpPost("simular-falha-interna")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult SimularFalhaInterna()
    {
        throw new InvalidOperationException("Falha simulada na integração com o bureau de crédito.");
    }
}
