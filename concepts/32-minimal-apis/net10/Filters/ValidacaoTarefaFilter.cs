using Microsoft.AspNetCore.Http;
using MinimalApisDemo.Models;

namespace MinimalApisDemo.Filters;

public class ValidacaoTarefaFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.GetArgument<CriarTarefaRequest>(0);

        if (request == null || string.IsNullOrWhiteSpace(request.Titulo))
        {
            return TypedResults.Problem(
                detail: "O titulo da tarefa e obrigatorio e nao pode ser vazio.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validacao de Entrada Falhou"
            );
        }

        var prioridadesValidas = new[] { "Baixa", "Media", "Alta" };
        if (!prioridadesValidas.Contains(request.Prioridade, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.Problem(
                detail: "A prioridade deve ser 'Baixa', 'Media' ou 'Alta'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Prioridade Invalida"
            );
        }

        return await next(context);
    }
}
