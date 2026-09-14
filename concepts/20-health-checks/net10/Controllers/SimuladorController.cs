using HealthChecksDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecksDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimuladorController : ControllerBase
{
    private readonly ISimuladorEstadoRecursos _simulador;

    public SimuladorController(ISimuladorEstadoRecursos simulador)
    {
        _simulador = simulador;
    }

    [HttpGet("estado")]
    public IActionResult ObterEstado()
    {
        return Ok(new
        {
            bancoDisponivel = _simulador.BancoDisponivel,
            bancoDegradado = _simulador.BancoDegradado,
            mensageriaDisponivel = _simulador.MensageriaDisponivel,
            limiteMemoriaMb = _simulador.LimiteMemoriaMegabytes
        });
    }

    [HttpPost("configurar-banco")]
    public IActionResult ConfigurarBanco([FromBody] ConfigurarBancoRequest request)
    {
        _simulador.BancoDisponivel = request.Disponivel;
        _simulador.BancoDegradado = request.Degradado;

        return Ok(new
        {
            mensagem = "Estado do banco de dados atualizado com sucesso",
            bancoDisponivel = _simulador.BancoDisponivel,
            bancoDegradado = _simulador.BancoDegradado
        });
    }

    [HttpPost("configurar-mensageria")]
    public IActionResult ConfigurarMensageria([FromBody] ConfigurarMensageriaRequest request)
    {
        _simulador.MensageriaDisponivel = request.Disponivel;

        return Ok(new
        {
            mensagem = "Estado do serviço de mensageria atualizado com sucesso",
            mensageriaDisponivel = _simulador.MensageriaDisponivel
        });
    }

    [HttpPost("resetar")]
    public IActionResult Resetar()
    {
        _simulador.BancoDisponivel = true;
        _simulador.BancoDegradado = false;
        _simulador.MensageriaDisponivel = true;

        return Ok(new
        {
            mensagem = "Todos os recursos foram restaurados para o estado saudável (Healthy)"
        });
    }

    public record ConfigurarBancoRequest(bool Disponivel, bool Degradado);
    public record ConfigurarMensageriaRequest(bool Disponivel);
}
