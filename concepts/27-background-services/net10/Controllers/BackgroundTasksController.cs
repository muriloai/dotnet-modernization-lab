using BackgroundServicesDemo.Models;
using BackgroundServicesDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundServicesDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackgroundTasksController : ControllerBase
{
    private readonly IFilaProcessamento _fila;
    private readonly EstadoMonitoramento _estado;

    public BackgroundTasksController(IFilaProcessamento fila, EstadoMonitoramento estado)
    {
        _fila = fila;
        _estado = estado;
    }

    [HttpGet("tarefas")]
    public IActionResult ObterTarefas()
    {
        var tarefas = _fila.ObterHistorico();
        return Ok(tarefas);
    }

    [HttpPost("enfileirar")]
    public async Task<IActionResult> EnfileirarTarefa([FromBody] CriarTarefaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao))
        {
            return BadRequest(new { Mensagem = "A descricao da tarefa e obrigatoria." });
        }

        var tarefa = new TarefaSegundoPlano
        {
            Descricao = request.Descricao.Trim()
        };

        await _fila.EnfileirarAsync(tarefa, cancellationToken);
        return Accepted(tarefa);
    }

    [HttpGet("metricas")]
    public IActionResult ObterMetricas()
    {
        var historico = _fila.ObterHistorico();
        var metricas = new MetricasWorkerResponse(
            TotalEnfileiradas: historico.Count,
            TotalConcluidas: historico.Count(t => t.Status == "Concluido"),
            TotalEmProcessamento: historico.Count(t => t.Status == "Em Processamento"),
            TotalTicksTimer: _estado.TotalTicks,
            UltimoTickTimer: _estado.UltimoTick,
            MemoriaAlocadaBytes: GC.GetTotalMemory(false)
        );

        return Ok(metricas);
    }
}
