namespace BackgroundServicesDemo.Services;

public class EstadoMonitoramento
{
    private int _totalTicks;
    private DateTime _ultimoTick = DateTime.UtcNow;

    public int TotalTicks => _totalTicks;
    public DateTime UltimoTick => _ultimoTick;

    public void RegistrarTick()
    {
        Interlocked.Increment(ref _totalTicks);
        _ultimoTick = DateTime.UtcNow;
    }
}
