using ActionFiltersDemo.Services;

namespace ActionFiltersDemo.Endpoints;

/// <summary>
/// Mapeamento de rotas demonstrando Endpoint Filters no .NET 10.
/// Nas Minimal APIs, em vez de herdar de ActionFilterAttribute ou usar [ServiceFilter],
/// a interceptação é realizada através do método AddEndpointFilter com delegates leves e assíncronos.
/// </summary>
public static class RelatoriosEndpoints
{
    public static RouteGroupBuilder MapRelatoriosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v2/relatorios")
            .WithTags("Relatórios (Minimal API com Endpoint Filter)");

        group.MapGet("/financeiro-protegido", () =>
        {
            return Results.Ok(new
            {
                Tipo = "Relatório Minimal API Protegido",
                Origem = "EndpointFilter funcional no .NET 10",
                Status = "Acesso autorizado com sucesso!"
            });
        })
        .AddEndpointFilter(async (invocationContext, next) =>
        {
            // Resolução da dependência diretamente pelo provedor de serviços da requisição
            var httpContext = invocationContext.HttpContext;
            var validator = httpContext.RequestServices.GetRequiredService<IApiKeyValidatorService>();

            var apiKey = httpContext.Request.Headers["X-API-Key"].FirstOrDefault();

            if (!validator.ValidarChave(apiKey))
            {
                // Curto-circuito equivalente nas Minimal APIs
                return Results.Json(new
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Erro = "Acesso Negado via Endpoint Filter",
                    Mensagem = "Bloqueado pelo delegate de EndpointFilter. Chave 'X-API-Key' inválida ou ausente.",
                    ChaveEsperada = validator.ChavePadraoEsperada
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // Prossegue para o manipulador do endpoint
            return await next(invocationContext);
        });

        return group;
    }
}
