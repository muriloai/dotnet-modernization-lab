using LoggingDemo.Models;

namespace LoggingDemo.Services;

public interface IProvedorLogsMemoria
{
    IReadOnlyList<LogItemVisualizacao> ObterLogs();
    void AdicionarLog(LogItemVisualizacao item);
    void Limpar();
}
