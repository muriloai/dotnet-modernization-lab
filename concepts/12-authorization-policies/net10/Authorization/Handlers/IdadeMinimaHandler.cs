using AuthorizationPoliciesDemo.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Handlers;

/// <summary>
/// Handler responsável por avaliar a política de idade mínima a partir das Claims do usuário.
/// No .NET 10, handlers são registrados com injeção de dependência e desacoplados do controller.
/// </summary>
public sealed class IdadeMinimaHandler : AuthorizationHandler<IdadeMinimaRequirement>
{
    private readonly ILogger<IdadeMinimaHandler> _logger;

    public IdadeMinimaHandler(ILogger<IdadeMinimaHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IdadeMinimaRequirement requirement)
    {
        var idadeClaim = context.User.FindFirst("idade");
        if (idadeClaim is not null && int.TryParse(idadeClaim.Value, out var idade))
        {
            if (idade >= requirement.IdadeMinima)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            _logger.LogInformation("Usuário com {Idade} anos não atende a idade mínima de {Minimo} anos.",
                idade, requirement.IdadeMinima);
        }

        return Task.CompletedTask;
    }
}
