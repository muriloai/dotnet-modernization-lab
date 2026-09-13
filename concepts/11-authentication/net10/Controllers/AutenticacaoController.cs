using System.Security.Claims;
using AuthenticationDemo.Models;
using AuthenticationDemo.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDemo.Controllers;

/// <summary>
/// Controller responsável pelos fluxos de autenticação moderna via Cookie e JWT.
/// Demonstra a coexistência de múltiplos esquemas e o uso de ClaimsPrincipal.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AutenticacaoController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenService _tokenService;

    public AutenticacaoController(IUsuarioService usuarioService, ITokenService tokenService)
    {
        _usuarioService = usuarioService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Retorna as contas de demonstração disponíveis no laboratório.
    /// </summary>
    [HttpGet("usuarios-demo")]
    public ActionResult<IEnumerable<object>> ObterUsuariosDemo()
    {
        var usuarios = _usuarioService.ListarUsuariosDemonstracao()
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Senha,
                u.Perfil,
                u.Departamento
            });

        return Ok(usuarios);
    }

    /// <summary>
    /// Realiza a autenticação via Cookie (Sessão de Navegador).
    /// Utiliza HttpContext.SignInAsync com o esquema CookieAuthenticationDefaults.
    /// </summary>
    [HttpPost("login-cookie")]
    public async Task<IActionResult> LoginCookie([FromBody] LoginRequest request)
    {
        var usuario = await _usuarioService.ValidarCredenciaisAsync(request.Email, request.Senha);
        if (usuario is null)
        {
            return Problem(
                detail: "E-mail ou senha incorretos para a conta informada.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciais Inválidas"
            );
        }

        // Construção explícita de Claims estruturadas
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil),
            new("departamento", usuario.Departamento),
            new("metodo_login", "cookie_aspnetcore"),
            new("horario_login_utc", DateTime.UtcNow.ToString("O"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.LembrarMe,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        // Emissão do cookie gerenciado pela Data Protection API
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties
        );

        return Ok(new
        {
            Mensagem = "Autenticação via Cookie realizada com êxito no .NET 10!",
            Esquema = CookieAuthenticationDefaults.AuthenticationScheme,
            Usuario = new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil,
                usuario.Departamento
            }
        });
    }

    /// <summary>
    /// Realiza a autenticação para APIs retornando um token JWT Bearer assinado.
    /// </summary>
    [HttpPost("login-jwt")]
    public async Task<ActionResult<TokenResponse>> LoginJwt([FromBody] LoginRequest request)
    {
        var usuario = await _usuarioService.ValidarCredenciaisAsync(request.Email, request.Senha);
        if (usuario is null)
        {
            return Problem(
                detail: "E-mail ou senha incorretos para a conta informada.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciais Inválidas"
            );
        }

        var tokenResponse = _tokenService.GerarToken(usuario);
        return Ok(tokenResponse);
    }

    /// <summary>
    /// Encerra a sessão de autenticação do usuário revogando o cookie.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { Mensagem = "Sessão encerrada com sucesso via HttpContext.SignOutAsync." });
    }

    /// <summary>
    /// Inspeciona o estado da identidade atual associada à requisição HTTP (via Cookie ou Token).
    /// </summary>
    [HttpGet("status")]
    public IActionResult ObterStatus()
    {
        var estaAutenticado = User.Identity?.IsAuthenticated ?? false;

        if (!estaAutenticado)
        {
            return Ok(new
            {
                Autenticado = false,
                Mensagem = "Nenhuma identidade autenticada detectada na requisição."
            });
        }

        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

        return Ok(new
        {
            Autenticado = true,
            Nome = User.Identity?.Name,
            TipoAutenticacao = User.Identity?.AuthenticationType,
            Claims = claims
        });
    }
}
