using AuthorizationPoliciesDemo.Models;
using AuthorizationPoliciesDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationPoliciesDemo.Controllers;

/// <summary>
/// Controller que demonstra múltiplos estilos de autorização no .NET 10:
/// - [Authorize] básico
/// - [Authorize(Roles)] clássico
/// - [Authorize(Policy)] com claims
/// - [Authorize(Policy)] com requisitos e handlers desacoplados
/// - Autorização imperativa baseada em recursos (Resource-Based Authorization)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DocumentosController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IRelatorioService _relatorioService;

    public DocumentosController(
        IAuthorizationService authorizationService,
        IRelatorioService relatorioService)
    {
        _authorizationService = authorizationService;
        _relatorioService = relatorioService;
    }

    /// <summary>
    /// Nível 1: Exige apenas que o usuário esteja autenticado.
    /// </summary>
    [HttpGet("painel-geral")]
    [Authorize]
    public IActionResult ObterPainelGeral()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Politica = "[Authorize] Autenticado",
            Mensagem = "Acesso concedido a qualquer usuário devidamente autenticado.",
            Usuario = User.Identity?.Name
        });
    }

    /// <summary>
    /// Nível 2: Autorização clássica baseada em papéis (Role-Based).
    /// </summary>
    [HttpGet("apenas-lideranca")]
    [Authorize(Roles = "Administrador,Gerente")]
    public IActionResult ObterAreaLideranca()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Politica = "[Authorize(Roles = 'Administrador,Gerente')]",
            Mensagem = "Acesso exclusivo aos papéis de Gerência ou Administração.",
            Usuario = User.Identity?.Name,
            Perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }

    /// <summary>
    /// Nível 3: Política baseada em Claim de departamento.
    /// </summary>
    [HttpGet("modulo-financeiro")]
    [Authorize(Policy = "ApenasFinanceiro")]
    public IActionResult ObterModuloFinanceiro()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Politica = "ApenasFinanceiro (RequireClaim('departamento', 'Financeiro'))",
            Mensagem = "Acesso concedido aos integrantes do Departamento Financeiro.",
            Departamento = User.FindFirst("departamento")?.Value
        });
    }

    /// <summary>
    /// Nível 4: Política com Requisito Customizado desacoplado (IdadeMinimaRequirement).
    /// Estagiário de 17 anos recebe 403 Forbidden!
    /// </summary>
    [HttpGet("assinatura-contratos")]
    [Authorize(Policy = "MaioridadeLegal")]
    public IActionResult ObterAssinaturaContratos()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Politica = "MaioridadeLegal (IdadeMinimaRequirement(18) via IdadeMinimaHandler)",
            Mensagem = "Usuário possui maioridade legal comprovada para assinar contratos corporativos.",
            Idade = User.FindFirst("idade")?.Value
        });
    }

    /// <summary>
    /// Nível 5: Política composta combinando Papéis, Claims e Requisitos customizados.
    /// Exige: Administrador/Gerente + Departamento Financeiro + AnosExperiencia >= 5.
    /// </summary>
    [HttpGet("aprovacoes-vultosas")]
    [Authorize(Policy = "AprovadorSenior")]
    public IActionResult ObterAprovacoesVultosas()
    {
        return Ok(new
        {
            Status = "Sucesso",
            Politica = "AprovadorSenior (Roles + Claim Depto + ExperienciaMinima >= 5)",
            Mensagem = "Permissão concedida para liberação de despesas vultosas.",
            Usuario = User.Identity?.Name,
            AnosExperiencia = User.FindFirst("anos_experiencia")?.Value
        });
    }

    /// <summary>
    /// Lista os relatórios disponíveis no repositório para demonstração de autorização de recurso.
    /// </summary>
    [HttpGet("relatorios")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RelatorioFinanceiro>>> ListarRelatorios()
    {
        var relatorios = await _relatorioService.ListarTodosAsync();
        return Ok(relatorios);
    }

    /// <summary>
    /// Nível 6: Autorização Baseada em Recursos (Resource-Based Authorization).
    /// Avalia a permissão diretamente sobre a entidade do relatório em tempo de execução.
    /// O autor original pode alterar; Administrador pode alterar; outros usuários recebem 403 Forbidden.
    /// </summary>
    [HttpPut("relatorios/{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> AtualizarStatusRelatorio(int id, [FromBody] string novoStatus)
    {
        var relatorio = await _relatorioService.ObterPorIdAsync(id);
        if (relatorio is null)
        {
            return NotFound(new { Mensagem = $"Relatório {id} não foi localizado." });
        }

        // Execução imperativa da política sobre o recurso concreto
        var resultadoAuth = await _authorizationService.AuthorizeAsync(User, relatorio, "DonoOuAdministrador");

        if (!resultadoAuth.Succeeded)
        {
            return Forbid(); // Retorna 403 Forbidden conforme o padrão HTTP
        }

        await _relatorioService.AtualizarStatusAsync(id, novoStatus ?? "Aprovado");

        return Ok(new
        {
            Status = "Sucesso",
            Mensagem = $"Relatório {id} atualizado com sucesso para '{novoStatus}'!",
            Relatorio = relatorio
        });
    }
}
