using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitDemo.Models;
using AsyncAwaitDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAwaitDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsyncAwaitController(OperacoesAssincronasModernoService service) : ControllerBase
{
    [HttpGet("stream")]
    public IAsyncEnumerable<EventoStream> ObterStream(
        [FromQuery] int total = 8,
        [FromQuery] int intervaloMs = 250,
        CancellationToken cancellationToken = default)
    {
        if (total < 1 || total > 50) total = 8;
        if (intervaloMs < 50 || intervaloMs > 2000) intervaloMs = 250;

        return service.GerarStreamAsync(total, intervaloMs, cancellationToken);
    }

    [HttpGet("comparar-alocacoes")]
    public async Task<IActionResult> CompararAlocacoes([FromQuery] int repeticoes = 100000)
    {
        if (repeticoes < 1000 || repeticoes > 500000) repeticoes = 100000;

        // Medição Task
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long bytesAntesTask = GC.GetAllocatedBytesForCurrentThread();
        var swTask = Stopwatch.StartNew();

        for (int i = 0; i < repeticoes; i++)
        {
            string _ = await service.ConsultarComTask("chave");
        }

        swTask.Stop();
        long bytesDepoisTask = GC.GetAllocatedBytesForCurrentThread();

        // Medição ValueTask
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long bytesAntesValueTask = GC.GetAllocatedBytesForCurrentThread();
        var swValueTask = Stopwatch.StartNew();

        for (int i = 0; i < repeticoes; i++)
        {
            string _ = await service.ConsultarComValueTask("chave");
        }

        swValueTask.Stop();
        long bytesDepoisValueTask = GC.GetAllocatedBytesForCurrentThread();

        long bytesTask = bytesDepoisTask - bytesAntesTask;
        long bytesValueTask = bytesDepoisValueTask - bytesAntesValueTask;

        return Ok(new
        {
            Repeticoes = repeticoes,
            ModoTask = new
            {
                Tipo = "Task<T> (Objeto de Referência na Heap)",
                TempoMs = Math.Round(swTask.Elapsed.TotalMilliseconds, 2),
                BytesAlocados = bytesTask,
                MegabytesAlocados = Math.Round((double)bytesTask / (1024.0 * 1024.0), 2)
            },
            ModoValueTask = new
            {
                Tipo = "ValueTask<T> (Tipo de Valor na Pilha)",
                TempoMs = Math.Round(swValueTask.Elapsed.TotalMilliseconds, 2),
                BytesAlocados = bytesValueTask,
                MegabytesAlocados = Math.Round((double)bytesValueTask / (1024.0 * 1024.0), 2)
            },
            Diagnostico = "ValueTask<T> evita alocações na Heap quando o resultado já está disponível de forma síncrona (como em acessos a cache em memória)."
        });
    }

    [HttpGet("when-each")]
    public IAsyncEnumerable<string> ObterEmOrdemDeConclusao(CancellationToken cancellationToken)
    {
        // Latências arbitrárias e desordenadas para demonstrar ordem de finalização
        var latencias = new List<int> { 600, 200, 900, 150, 400 };
        return service.ProcessarComWhenEachAsync(latencias, cancellationToken);
    }
}
