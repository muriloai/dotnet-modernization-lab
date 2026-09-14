using BackgroundServicesDemo.Models;
using BackgroundServicesDemo.Services;

namespace BackgroundServicesDemo.Workers;

public class FilaProcessamentoWorker : BackgroundService
{
    private readonly IFilaProcessamento _fila;
    private readonly ILogger<FilaProcessamentoWorker> _logger;

    public FilaProcessamentoWorker(IFilaProcessamento fila, ILogger<FilaProcessamentoWorker> logger)
    {
        _fila = fila;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FilaProcessamentoWorker iniciado. Aguardando tarefas no Channel...");

        try
        {
            await foreach (var tarefa in _fila.LerTarefasAsync(stoppingToken))
            {
                _logger.LogInformation("Processando tarefa {Id}: {Descricao}", tarefa.Id, tarefa.Descricao);
                
                tarefa.IniciadoEm = DateTime.UtcNow;
                tarefa.Status = "Em Processamento";
                tarefa.ProgressoPercentual = 10;
                _fila.AtualizarTarefa(tarefa);

                for (int p = 25; p <= 100; p += 25)
                {
                    await Task.Delay(400, stoppingToken);
                    tarefa.ProgressoPercentual = p;
                    _fila.AtualizarTarefa(tarefa);
                }

                tarefa.ConcluidoEm = DateTime.UtcNow;
                tarefa.Status = "Concluido";
                tarefa.Resultado = $"Lote processado com sucesso em {(tarefa.ConcluidoEm.Value - tarefa.IniciadoEm.Value).TotalMilliseconds:N0} ms";
                _fila.AtualizarTarefa(tarefa);

                _logger.LogInformation("Tarefa {Id} finalizada com status: {Status}", tarefa.Id, tarefa.Status);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("FilaProcessamentoWorker recebeu sinal de cancelamento (graceful shutdown).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no FilaProcessamentoWorker.");
        }
    }
}
