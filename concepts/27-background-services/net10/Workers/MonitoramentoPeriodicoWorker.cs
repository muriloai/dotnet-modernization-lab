using BackgroundServicesDemo.Services;

namespace BackgroundServicesDemo.Workers;

public class MonitoramentoPeriodicoWorker : BackgroundService
{
    private readonly EstadoMonitoramento _estado;
    private readonly ILogger<MonitoramentoPeriodicoWorker> _logger;

    public MonitoramentoPeriodicoWorker(EstadoMonitoramento estado, ILogger<MonitoramentoPeriodicoWorker> logger)
    {
        _estado = estado;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonitoramentoPeriodicoWorker iniciado com PeriodicTimer a cada 3 segundos.");
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _estado.RegistrarTick();
                long memoria = GC.GetTotalMemory(false);
                _logger.LogDebug("PeriodicTimer Tick #{Tick}. Memoria alocada: {Memoria} bytes", _estado.TotalTicks, memoria);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MonitoramentoPeriodicoWorker finalizado graciosamente via CancellationToken.");
        }
    }
}
