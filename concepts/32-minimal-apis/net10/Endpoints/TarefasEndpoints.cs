using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApisDemo.Filters;
using MinimalApisDemo.Models;

namespace MinimalApisDemo.Endpoints;

public static class TarefasEndpoints
{
    private static readonly List<Tarefa> _tarefas = new()
    {
        new Tarefa(1, "Migrar Web.config para appsettings.json", true, "Alta", DateTime.UtcNow.AddHours(-3)),
        new Tarefa(2, "Substituir WCF por gRPC ou REST Minimal APIs", false, "Alta", DateTime.UtcNow.AddHours(-2)),
        new Tarefa(3, "Configurar imagem Docker Chiseled para deploy", false, "Media", DateTime.UtcNow.AddHours(-1))
    };

    public static RouteGroupBuilder MapTarefasEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", ObterTodas);
        group.MapGet("/{id:int}", ObterPorId);
        group.MapPost("/", Criar).AddEndpointFilter<ValidacaoTarefaFilter>();
        group.MapPut("/{id:int}/alternar", AlternarConclusao);
        group.MapDelete("/{id:int}", Excluir);

        return group;
    }

    public static Ok<List<Tarefa>> ObterTodas()
    {
        return TypedResults.Ok(_tarefas.OrderBy(t => t.Concluida).ThenByDescending(t => t.Id).ToList());
    }

    public static Results<Ok<Tarefa>, NotFound<string>> ObterPorId(int id)
    {
        var tarefa = _tarefas.FirstOrDefault(t => t.Id == id);
        return tarefa is not null
            ? TypedResults.Ok(tarefa)
            : TypedResults.NotFound($"Tarefa com ID {id} nao encontrada.");
    }

    public static Created<Tarefa> Criar(CriarTarefaRequest request)
    {
        var novoId = _tarefas.Count > 0 ? _tarefas.Max(t => t.Id) + 1 : 1;
        var novaTarefa = new Tarefa(novoId, request.Titulo.Trim(), false, request.Prioridade, DateTime.UtcNow);
        _tarefas.Add(novaTarefa);

        return TypedResults.Created($"/api/tarefas/{novoId}", novaTarefa);
    }

    public static Results<Ok<Tarefa>, NotFound<string>> AlternarConclusao(int id)
    {
        var idx = _tarefas.FindIndex(t => t.Id == id);
        if (idx == -1)
        {
            return TypedResults.NotFound($"Tarefa com ID {id} nao encontrada.");
        }

        var atual = _tarefas[idx];
        var atualizada = atual with { Concluida = !atual.Concluida };
        _tarefas[idx] = atualizada;

        return TypedResults.Ok(atualizada);
    }

    public static Results<NoContent, NotFound<string>> Excluir(int id)
    {
        var removidos = _tarefas.RemoveAll(t => t.Id == id);
        return removidos > 0
            ? TypedResults.NoContent()
            : TypedResults.NotFound($"Tarefa com ID {id} nao encontrada.");
    }
}
