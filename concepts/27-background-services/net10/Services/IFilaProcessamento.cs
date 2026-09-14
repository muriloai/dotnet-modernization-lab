using BackgroundServicesDemo.Models;

namespace BackgroundServicesDemo.Services;

public interface IFilaProcessamento
{
    ValueTask EnfileirarAsync(TarefaSegundoPlano tarefa, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TarefaSegundoPlano> LerTarefasAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<TarefaSegundoPlano> ObterHistorico();
    TarefaSegundoPlano? ObterPorId(Guid id);
    void AtualizarTarefa(TarefaSegundoPlano tarefa);
}
