using AuthorizationPoliciesDemo.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Handlers;

/// <summary>
/// Handler responsável por avaliar a experiência mínima exigida por uma política de autorização.
/// </summary>
public sealed class ExperienciaMinimaHandler : AuthorizationHandler<ExperienciaMinimaRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ExperienciaMinimaRequirement requirement)
    {
        var expClaim = context.User.FindFirst("anos_experiencia");
        if (expClaim is not null && int.TryParse(expClaim.Value, out var anos))
        {
            if (anos >= requirement.AnosMinimos)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
