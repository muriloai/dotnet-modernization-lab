using System.Collections.Concurrent;
using System.Threading.Channels;
using BackgroundServicesDemo.Models;

namespace BackgroundServicesDemo.Services;

public class FilaProcessamentoChannel : IFilaProcessamento
{
    private readonly Channel<TarefaSegundoPlano> _channel;
    private readonly ConcurrentDictionary<Guid, TarefaSegundoPlano> _historico = new();

    public FilaProcessamentoChannel()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateBounded<TarefaSegundoPlano>(options);
    }

    public async ValueTask EnfileirarAsync(TarefaSegundoPlano tarefa, CancellationToken cancellationToken = default)
    {
        _historico[tarefa.Id] = tarefa;
        await _channel.Writer.WriteAsync(tarefa, cancellationToken);
    }

    public IAsyncEnumerable<TarefaSegundoPlano> LerTarefasAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    public IReadOnlyList<TarefaSegundoPlano> ObterHistorico()
    {
        return _historico.Values.OrderByDescending(t => t.CriadoEm).ToList();
    }

    public TarefaSegundoPlano? ObterPorId(Guid id)
    {
        return _historico.TryGetValue(id, out var tarefa) ? tarefa : null;
    }

    public void AtualizarTarefa(TarefaSegundoPlano tarefa)
    {
        _historico[tarefa.Id] = tarefa;
    }
}
