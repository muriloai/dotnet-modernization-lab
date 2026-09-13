using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Requirements;

/// <summary>
/// Requisito marcador para autorização baseada em recursos (Resource-Based Authorization).
/// Determina se o usuário é o proprietário do relatório ou possui papel administrativo.
/// </summary>
public sealed class DonoDoRelatorioRequirement : IAuthorizationRequirement
{
}
