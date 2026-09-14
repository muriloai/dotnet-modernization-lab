using EnvironmentDemo.Models;
using EnvironmentDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureManagerController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly GerenciadorFlagsEmMemoria _flags;

    private static readonly List<(string Nome, string Descricao)> _featuresCadastradas = new()
    {
        ("NovoCheckout", "Habilita o fluxo moderno de checkout V2"),
        ("DescontoBlackFriday", "Aplica 20% de desconto adicional na finalizacao"),
        ("ModoBetaRelatorios", "Exibe abas de relatorios analiticos avancados")
    };

    public FeatureManagerController(IWebHostEnvironment env, IConfiguration config, GerenciadorFlagsEmMemoria flags)
    {
        _env = env;
        _config = config;
        _flags = flags;
    }

    [HttpGet("ambiente")]
    public async Task<IActionResult> ObterInfoAmbiente()
    {
        var lista = new List<FeatureItemDto>();

        foreach (var f in _featuresCadastradas)
        {
            bool ativa = await _flags.IsEnabledAsync(f.Nome);
            lista.Add(new FeatureItemDto(f.Nome, ativa, f.Descricao));
        }

        var dto = new AmbienteInfoDto(
            AmbienteNome: _env.EnvironmentName,
            IsDevelopment: _env.IsDevelopment(),
            IsProduction: _env.IsProduction(),
            AmbienteConfig: _config["AmbienteConfig"] ?? "Nao configurado",
            Features: lista
        );

        return Ok(dto);
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> AlternarFeature([FromBody] ToggleFeatureRequest req)
    {
        _flags.DefinirOverride(req.FeatureName, req.Enabled);
        return await ObterInfoAmbiente();
    }
}
