using EnvironmentDemo.Models;
using EnvironmentDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly GerenciadorFlagsEmMemoria _flags;

    public CheckoutController(GerenciadorFlagsEmMemoria flags)
    {
        _flags = flags;
    }

    [HttpPost("processar")]
    public async Task<IActionResult> ProcessarCheckout()
    {
        bool novoCheckoutAtivo = await _flags.IsEnabledAsync("NovoCheckout");
        bool descontoAtivo = await _flags.IsEnabledAsync("DescontoBlackFriday");

        if (novoCheckoutAtivo)
        {
            return Ok(new CheckoutResponse(
                Mensagem: "Checkout processado com o novo motor unificado V2 em tempo real!",
                VersaoCheckout: "V2 (Moderno com PIX e Cartao)",
                DescontoAplicado: descontoAtivo,
                ProcessadoEm: DateTime.UtcNow
            ));
        }

        return Ok(new CheckoutResponse(
            Mensagem: "Checkout processado pelo fluxo legado V1 (Boleto/Cartao simples).",
            VersaoCheckout: "V1 (Legado)",
            DescontoAplicado: false,
            ProcessadoEm: DateTime.UtcNow
        ));
    }
}
