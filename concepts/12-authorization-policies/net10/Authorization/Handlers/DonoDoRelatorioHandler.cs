using System.Security.Claims;
using AuthorizationPoliciesDemo.Authorization.Requirements;
using AuthorizationPoliciesDemo.Models;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizationPoliciesDemo.Authorization.Handlers;

/// <summary>
/// Handler de autorização baseada em recurso.
/// Avalia as permissões do usuário diretamente sobre a instância concreta do relatório financeiro.
/// </summary>
public sealed class DonoDoRelatorioHandler : AuthorizationHandler<DonoDoRelatorioRequirement, RelatorioFinanceiro>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DonoDoRelatorioRequirement requirement,
        RelatorioFinanceiro resource)
    {
        // Administradores possuem acesso total a qualquer relatório
        if (context.User.IsInRole("Administrador"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Usuários comuns só podem atuar sobre relatórios criados por eles mesmos
        var emailUsuario = context.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(emailUsuario) &&
            resource.AutorEmail.Equals(emailUsuario, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
