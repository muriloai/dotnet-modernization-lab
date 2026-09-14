using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using BackgroundServicesDemo.Models;

namespace BackgroundServicesDemo.Services
{
    public static class FilaProcessamentoLegada
    {
        private static readonly ConcurrentDictionary<Guid, TarefaSegundoPlanoLegada> _historico =
            new ConcurrentDictionary<Guid, TarefaSegundoPlanoLegada>();

        private static Timer _timerMonitoramento;
        private static int _totalTicks;
        private static DateTime _ultimoTick = DateTime.Now;

        public static int TotalTicks => _totalTicks;
        public static DateTime UltimoTick => _ultimoTick;

        public static void IniciarTimerMonitoramento()
        {
            // Anti-padrao comum no legado: Timer executando em threadpool descontrolado pelo IIS
            _timerMonitoramento = new Timer(OnTimerTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
        }

        public static void PararTimerMonitoramento()
        {
            _timerMonitoramento?.Dispose();
        }

        private static void OnTimerTick(object state)
        {
            Interlocked.Increment(ref _totalTicks);
            _ultimoTick = DateTime.Now;
        }

        public static void EnfileirarComQueueBackgroundWorkItem(string descricao)
        {
            var tarefa = new TarefaSegundoPlanoLegada
            {
                Descricao = descricao
            };
            _historico[tarefa.Id] = tarefa;

            // Uso do QueueBackgroundWorkItem introduzido no .NET 4.5.2
            HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
            {
                try
                {
                    tarefa.Status = "Em Processamento";
                    tarefa.ProgressoPercentual = 15;

                    for (int p = 25; p <= 100; p += 25)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tarefa.Status = "Cancelado pelo IIS";
                            tarefa.Resultado = "AppPool foi reciclado ou o host solicitou encerramento.";
                            return;
                        }

                        await System.Threading.Tasks.Task.Delay(400, cancellationToken);
                        tarefa.ProgressoPercentual = p;
                    }

                    tarefa.Status = "Concluido";
                    tarefa.Resultado = "Processamento em segundo plano finalizado no .NET 4.8.1.";
                }
                catch (OperationCanceledException)
                {
                    tarefa.Status = "Cancelado pelo IIS";
                    tarefa.Resultado = "Cancelamento disparado pelo HostingEnvironment.";
                }
                catch (Exception ex)
                {
                    tarefa.Status = "Falha";
                    tarefa.Resultado = "Erro: " + ex.Message;
                }
            });
        }

        public static List<TarefaSegundoPlanoLegada> ObterTarefas()
        {
            return _historico.Values.OrderByDescending(t => t.CriadoEm).ToList();
        }
    }
}
