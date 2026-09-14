using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitDemo.Models;

namespace AsyncAwaitDemo.Services;

public class OperacoesAssincronasModernoService
{
    // Streaming Assíncrono com IAsyncEnumerable elemento a elemento
    public async IAsyncEnumerable<EventoStream> GerarStreamAsync(
        int total,
        int intervaloMs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= total; i++)
        {
            await Task.Delay(intervaloMs, cancellationToken);
            yield return new EventoStream(
                i,
                $"Evento de telemetria #{i:D4}",
                DateTime.Now,
                intervaloMs
            );
        }
    }

    // Retorno síncrono com Task (aloca objeto Task na Heap)
    public Task<string> ConsultarComTask(string chave)
    {
        return Task.FromResult($"Resultado_Task_{chave}");
    }

    // Retorno síncrono com ValueTask (alocação zero na Heap, struct na pilha)
    public ValueTask<string> ConsultarComValueTask(string chave)
    {
        return new ValueTask<string>($"Resultado_ValueTask_{chave}");
    }

    // Processamento sob demanda conforme cada tarefa termina usando Task.WhenEach
    public async IAsyncEnumerable<string> ProcessarComWhenEachAsync(
        List<int> latenciasMs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var tarefas = latenciasMs.Select(async ms =>
        {
            await Task.Delay(ms, cancellationToken);
            return $"Operação ({ms} ms) finalizada às {DateTime.Now:HH:mm:ss.fff}";
        }).ToList();

        // Task.WhenEach itera conforme as tarefas completam, sem ordem pré-fixada
        await foreach (var task in Task.WhenEach(tarefas))
        {
            string resultado = await task;
            yield return resultado;
        }
    }
}
