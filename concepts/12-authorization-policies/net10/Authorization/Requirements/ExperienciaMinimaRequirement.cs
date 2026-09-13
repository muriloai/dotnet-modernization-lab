using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Requirements;

/// <summary>
/// Requisito que exige tempo mínimo de experiência profissional comprovada.
/// </summary>
public sealed class ExperienciaMinimaRequirement : IAuthorizationRequirement
{
    public int AnosMinimos { get; }

    public ExperienciaMinimaRequirement(int anosMinimos)
    {
        AnosMinimos = anosMinimos;
    }
}
