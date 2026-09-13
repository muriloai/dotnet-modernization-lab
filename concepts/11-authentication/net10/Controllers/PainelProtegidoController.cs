using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDemo.Controllers;

/// <summary>
/// Controller com recursos protegidos demonstrando o funcionamento de [Authorize],
/// compatibilidade transparente com Cookie e Bearer JWT e verificação de Claims de papéis.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class PainelProtegidoController : ControllerBase
{
    /// <summary>
    /// Retorna o perfil e todas as claims do usuário autenticado no momento.
    /// Funciona de forma idêntica tanto para requisições com Cookie quanto com JWT Bearer.
    /// </summary>
    [HttpGet("meu-perfil")]
    public IActionResult ObterMeuPerfil()
    {
        var claimsLista = User.Claims.Select(c => new
        {
            Tipo = c.Type,
            Valor = c.Value,
            Emissor = c.Issuer
        }).ToList();

        var departamentoClaim = User.FindFirst("departamento")?.Value ?? "Não informado";
        var tipoEmissao = User.FindFirst("tipo_emissao")?.Value ?? "cookie_ou_sessao";

        return Ok(new
        {
            Mensagem = "Acesso autorizado com sucesso!",
            Identidade = new
            {
                Nome = User.Identity?.Name,
                EstaAutenticado = User.Identity?.IsAuthenticated,
                TipoAutenticacao = User.Identity?.AuthenticationType,
                Departamento = departamentoClaim,
                TipoEmissao = tipoEmissao
            },
            TotalClaims = claimsLista.Count,
            Claims = claimsLista
        });
    }

    /// <summary>
    /// Recurso restrito exclusivamente para o perfil Administrador.
    /// Caso um usuário com perfil Operador tente acessar, o ASP.NET Core retorna 403 Forbidden.
    /// </summary>
    [HttpGet("dados-restritos-admin")]
    [Authorize(Roles = "Administrador")]
    public IActionResult ObterDadosRestritosAdmin()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = "Dados confidenciais acessados com autorização de Administrador.",
            ServidoresAtivos = 42,
            ChaveMasterAtiva = "KMS-2026-PROD-SECURE",
            UltimaAuditoria = DateTime.UtcNow.AddHours(-2).ToString("O")
        });
    }

    /// <summary>
    /// Recurso acessível tanto por Operadores quanto por Administradores.
    /// </summary>
    [HttpGet("dados-operacionais")]
    [Authorize(Roles = "Administrador,Operador")]
    public IActionResult ObterDadosOperacionais()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = "Painel operacional acessado com sucesso.",
            ChamadosAbertos = 5,
            TempoMedioResposta = "12 minutos"
        });
    }
}
