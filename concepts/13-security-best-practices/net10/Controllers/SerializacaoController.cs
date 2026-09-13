using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Mvc;
using SecurityBestPracticesDemo.Models;
using SecurityBestPracticesDemo.Services;

namespace SecurityBestPracticesDemo.Controllers;

/// <summary>
/// Controller didático que compara a serialização segura com System.Text.Json
/// e demonstra o banimento e bloqueio do vulnerável BinaryFormatter no .NET 10.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SerializacaoController : ControllerBase
{
    private readonly ITransferenciaService _transferenciaService;

    public SerializacaoController(ITransferenciaService transferenciaService)
    {
        _transferenciaService = transferenciaService;
    }

    /// <summary>
    /// Demonstra a serialização segura tipada utilizando System.Text.Json.
    /// Opera com limites de profundidade e sem instanciação de tipos arbitrários.
    /// </summary>
    [HttpGet("json-seguro")]
    public IActionResult DemonstrarJsonSeguro()
    {
        var exemplo = new TransacaoRegistro(
            Id: Guid.NewGuid(),
            ContaOrigem: "111-22233-4",
            ContaDestino: "555-66677-8",
            Valor: 5400.00m,
            Descricao: "Pagamento de Serviços de Nuvem",
            DataUtc: DateTime.UtcNow,
            ProtocoloSeguranca: "System.Text.Json - Tipagem Segura contra RCE"
        );

        var json = _transferenciaService.SerializarTransacaoSegura(exemplo);
        var reconstituido = _transferenciaService.DesserializarTransacaoSegura(json);

        return Ok(new
        {
            Mensagem = "Serialização e desserialização tipadas e seguras concluídas com System.Text.Json.",
            JsonGerado = json,
            ObjetoReconstituido = reconstituido
        });
    }

    /// <summary>
    /// Demonstra o bloqueio intencional do BinaryFormatter no .NET 10.
    /// O runtime rejeita sua execução com PlatformNotSupportedException para impedir ataques de RCE.
    /// </summary>
    [HttpGet("tentativa-binary-formatter")]
    public IActionResult DemonstrarBloqueioBinaryFormatter()
    {
#pragma warning disable SYSLIB0011 // O BinaryFormatter está obsoleto e gera erro em tempo de execução
        try
        {
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
            formatter.Serialize(stream, "teste");

            return Ok(new { Mensagem = "Inesperado: BinaryFormatter executou." });
        }
        catch (PlatformNotSupportedException ex)
        {
            return Ok(new
            {
                Status = "Proteção Ativa do .NET 10",
                Mensagem = "O BinaryFormatter foi permanentemente desabilitado no .NET 10 devido a graves riscos de execução remota de código (RCE).",
                Excecao = ex.GetType().Name,
                Detalhe = ex.Message,
                Recomendacao = "Utilize System.Text.Json para serialização estruturada e segura."
            });
        }
#pragma warning restore SYSLIB0011
    }
}
