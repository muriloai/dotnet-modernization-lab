using System.Security.Claims;
using AuthorizationPoliciesDemo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationPoliciesDemo.Controllers;

/// <summary>
/// Controller para gerenciar a sessão ativa e alternar entre identidades com diferentes claims para teste das políticas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SessaoController : ControllerBase
{
    private static readonly List<UsuarioInfo> UsuariosDisponiveis = new()
    {
        new UsuarioInfo(1, "Ana Administradora", "admin@empresa.com", "Administrador", "Tecnologia", 42, 12, "Diretoria"),
        new UsuarioInfo(2, "Beatriz Gerente", "gerente.financeiro@empresa.com", "Gerente", "Financeiro", 36, 7, "Senior"),
        new UsuarioInfo(3, "Claudio Analista", "analista.financeiro@empresa.com", "Analista", "Financeiro", 27, 3, "Pleno"),
        new UsuarioInfo(4, "Daniel Estagiario", "estagiario.ti@empresa.com", "Estagiario", "Tecnologia", 17, 0, "Junior")
    };

    [HttpGet("usuarios-demo")]
    public ActionResult<IEnumerable<UsuarioInfo>> ListarUsuariosDemo()
    {
        return Ok(UsuariosDisponiveis);
    }

    [HttpPost("selecionar-usuario")]
    public async Task<IActionResult> SelecionarUsuario([FromBody] string email)
    {
        var usuario = UsuariosDisponiveis.FirstOrDefault(u =>
            u.Email.Equals(email?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (usuario is null)
        {
            return NotFound(new { Mensagem = "Usuário de demonstração não localizado." });
        }

        // Montagem das claims estruturadas que alimentam as políticas de autorização do .NET 10
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil),
            new("departamento", usuario.Departamento),
            new("idade", usuario.Idade.ToString()),
            new("anos_experiencia", usuario.AnosExperiencia.ToString()),
            new("nivel", usuario.Nivel)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new
        {
            Mensagem = $"Sessão iniciada como '{usuario.Nome}' ({usuario.Perfil}).",
            Usuario = usuario
        });
    }

    [HttpGet("usuario-ativo")]
    public IActionResult ObterUsuarioAtivo()
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return Ok(new
            {
                Autenticado = false,
                Mensagem = "Nenhum usuário autenticado no momento."
            });
        }

        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

        return Ok(new
        {
            Autenticado = true,
            Nome = User.Identity.Name,
            Perfil = User.FindFirst(ClaimTypes.Role)?.Value,
            Departamento = User.FindFirst("departamento")?.Value,
            Idade = User.FindFirst("idade")?.Value,
            AnosExperiencia = User.FindFirst("anos_experiencia")?.Value,
            Nivel = User.FindFirst("nivel")?.Value,
            Claims = claims
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { Mensagem = "Sessão encerrada com sucesso." });
    }
}
