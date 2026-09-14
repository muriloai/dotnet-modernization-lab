using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncAwaitDemo.Models;

namespace AsyncAwaitDemo.Services
{
    public class OperacoesAssincronasLegadoService
    {
        // Sem IAsyncEnumerable, coleções inteiras precisavam ser montadas na memória
        public async Task<List<EventoStreamLegado>> ObterLoteCompletoAsync(int total, int intervaloMs)
        {
            var lista = new List<EventoStreamLegado>();

            for (int i = 1; i <= total; i++)
            {
                // .ConfigureAwait(false) era mandatório para evitar deadlocks de sincronização
                await Task.Delay(intervaloMs).ConfigureAwait(false);

                lista.Add(new EventoStreamLegado(
                    i,
                    string.Format("Evento legado #{0:D4}", i),
                    DateTime.Now,
                    intervaloMs
                ));
            }

            return lista;
        }

        // Sem ValueTask, toda operação síncrona retornando Task alocava na Heap
        public Task<string> ConsultarComTask(string chave)
        {
            return Task.FromResult("Resultado_Legado_" + chave);
        }

        // Exemplo da armadilha clássica: bloquear Task em thread com AspNetSynchronizationContext
        public string SimularRiscoDeadlock()
        {
            // Em cenários legados, Task.Run(async () => ...).Result podia travar o IIS
            return "No ASP.NET clássico, invocar .Result ou .Wait() bloqueava a thread da requisição enquanto a Task aguardava o mesmo SynchronizationContext, travando o processo.";
        }
    }
}
