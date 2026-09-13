using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Requirements;

/// <summary>
/// Requisito que exige uma idade mínima para acesso.
/// </summary>
public sealed class IdadeMinimaRequirement : IAuthorizationRequirement
{
    public int IdadeMinima { get; }

    public IdadeMinimaRequirement(int idadeMinima)
    {
        IdadeMinima = idadeMinima;
    }
}
