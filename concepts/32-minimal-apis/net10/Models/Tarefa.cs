namespace MinimalApisDemo.Models;

public record Tarefa(
    int Id,
    string Titulo,
    bool Concluida,
    string Prioridade,
    DateTime CriadaEm
);

public record CriarTarefaRequest(
    string Titulo,
    string Prioridade
);
