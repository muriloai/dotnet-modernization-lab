using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SecurityBestPracticesDemo.Models;
using SecurityBestPracticesDemo.Services;

namespace SecurityBestPracticesDemo.Controllers;

/// <summary>
/// Controller moderno demonstrando as defesas em camadas do .NET 10:
/// - Prevenção contra CSRF via IAntiforgery
/// - Políticas restritivas de CORS
/// - Injeção de cabeçalhos de segurança HTTP
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SegurancaController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;
    private readonly ITransferenciaService _transferenciaService;

    public SegurancaController(IAntiforgery antiforgery, ITransferenciaService transferenciaService)
    {
        _antiforgery = antiforgery;
        _transferenciaService = transferenciaService;
    }

    /// <summary>
    /// Emite um token Anti-CSRF para clientes que efetuam requisições AJAX/Fetch.
    /// O token é gravado em cookie e retornado no corpo para ser enviado no cabeçalho X-XSRF-TOKEN.
    /// </summary>
    [HttpGet("token-antiforgery")]
    public IActionResult ObterTokenAntiforgery()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            Mensagem = "Token Anti-CSRF emitido com sucesso pelo serviço IAntiforgery.",
            HeaderName = "X-XSRF-TOKEN",
            RequestToken = tokens.RequestToken,
            CookieName = ".ModernLab.Antiforgery"
        });
    }

    /// <summary>
    /// Endpoint protegido contra CSRF.
    /// Valida o cabeçalho X-XSRF-TOKEN e o cookie correspondente antes de executar a operação financeira.
    /// </summary>
    [HttpPost("transferencia-protegida")]
    public async Task<IActionResult> TransferenciaProtegida([FromBody] TransferenciaRequest request)
    {
        try
        {
            // Validação explícita e controlada do token no pipeline do ASP.NET Core
            await _antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Problem(
                detail: "A requisição foi rejeitada por ausência ou invalidade do token Anti-CSRF.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Falha de Validação Anti-CSRF (400 Bad Request)",
                instance: HttpContext.Request.Path
            );
        }

        var registro = await _transferenciaService.ExecutarTransferenciaAsync(request);

        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = "Transferência processada com segurança e proteção Anti-CSRF validada!",
            Transacao = registro
        });
    }

    /// <summary>
    /// Endpoint deliberadamente desprotegido para demonstrar a vulnerabilidade quando CSRF não é tratado.
    /// </summary>
    [HttpPost("transferencia-sem-protecao")]
    public async Task<IActionResult> TransferenciaSemProtecao([FromBody] TransferenciaRequest request)
    {
        var registro = await _transferenciaService.ExecutarTransferenciaAsync(request);

        return Ok(new
        {
            Status = "Atenção: Vulnerável",
            Mensagem = "Transferência executada SEM validação de token Anti-CSRF.",
            Transacao = registro
        });
    }

    /// <summary>
    /// Retorna os cabeçalhos de segurança HTTP presentes na resposta desta requisição.
    /// </summary>
    [HttpGet("headers-seguranca")]
    public IActionResult InspecionarHeadersSeguranca()
    {
        var headers = new Dictionary<string, string>();
        foreach (var h in Response.Headers)
        {
            headers[h.Key] = h.Value.ToString();
        }

        return Ok(new
        {
            Mensagem = "Cabeçalhos de segurança HTTP ativos injetados pelo pipeline do .NET 10:",
            Headers = headers
        });
    }

    /// <summary>
    /// Endpoint protegido por política restritiva de CORS.
    /// </summary>
    [HttpGet("cors-restrito")]
    [EnableCors("PoliticaRestritaParceiro")]
    public IActionResult EndpointCorsRestrito()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = "Acesso autorizado pela política de CORS restrita (apenas origens explicitamente homologadas).",
            OrigemPermitida = "https://portal-parceiro.empresa.com.br"
        });
    }
}
