using System.Collections.Concurrent;
using LoggingDemo.Models;

namespace LoggingDemo.Services;

public class ProvedorLogsMemoria : ILoggerProvider, ISupportExternalScope, IProvedorLogsMemoria
{
    private readonly ConcurrentQueue<LogItemVisualizacao> _logs = new();
    private const int MaxLogs = 100;
    private IExternalScopeProvider? _scopeProvider;

    public IReadOnlyList<LogItemVisualizacao> ObterLogs()
    {
        return _logs.Reverse().ToList();
    }

    public void AdicionarLog(LogItemVisualizacao item)
    {
        _logs.Enqueue(item);
        while (_logs.Count > MaxLogs && _logs.TryDequeue(out _)) { }
    }

    public void Limpar()
    {
        _logs.Clear();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MemoriaLogger(categoryName, this, _scopeProvider);
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public void Dispose()
    {
        _logs.Clear();
        GC.SuppressFinalize(this);
    }

    private sealed class MemoriaLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ProvedorLogsMemoria _provedor;
        private readonly IExternalScopeProvider? _scopeProvider;

        public MemoriaLogger(string categoryName, ProvedorLogsMemoria provedor, IExternalScopeProvider? scopeProvider)
        {
            _categoryName = categoryName;
            _provedor = provedor;
            _scopeProvider = scopeProvider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _scopeProvider?.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_categoryName.StartsWith("Microsoft.AspNetCore.StaticFiles", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var mensagem = formatter(state, exception);
            var item = new LogItemVisualizacao
            {
                Nivel = logLevel.ToString(),
                Categoria = _categoryName,
                EventId = eventId.Id,
                Mensagem = mensagem,
                DataHora = DateTime.UtcNow
            };

            if (state is IEnumerable<KeyValuePair<string, object?>> propriedades)
            {
                foreach (var prop in propriedades)
                {
                    if (prop.Key != "{OriginalFormat}")
                    {
                        item.PropriedadesEstruturadas[prop.Key] = prop.Value;
                    }
                }
            }

            _scopeProvider?.ForEachScope((scope, _) =>
            {
                if (scope is IEnumerable<KeyValuePair<string, object?>> scopeProps)
                {
                    foreach (var prop in scopeProps)
                    {
                        if (prop.Key != "{OriginalFormat}")
                        {
                            item.PropriedadesEstruturadas[$"Escopo:{prop.Key}"] = prop.Value;
                        }
                    }
                }
                else if (scope != null)
                {
                    item.Escopos.Add(scope.ToString() ?? string.Empty);
                }
            }, (object?)null);

            if (exception != null)
            {
                item.PropriedadesEstruturadas["ExceptionType"] = exception.GetType().FullName;
                item.PropriedadesEstruturadas["ExceptionMessage"] = exception.Message;
            }

            _provedor.AdicionarLog(item);
        }
    }
}
