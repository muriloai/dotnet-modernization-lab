using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ActionFiltersDemo.Services;

namespace ActionFiltersDemo.Filters;

/// <summary>
/// Filtro de validação de segurança que demonstra a injeção de dependências no construtor.
/// No .NET 10, esse filtro é registrado no container com AddScoped e aplicado nos controllers
/// através do atributo [ServiceFilter(typeof(ValidarApiKeyServiceFilter))].
/// Se a validação falhar, o filtro executa um curto-circuito (short-circuit),
/// atribuindo um IActionResult a context.Result e impedindo a execução da Action.
/// </summary>
public sealed class ValidarApiKeyServiceFilter : IAsyncActionFilter
{
    private readonly IApiKeyValidatorService _apiKeyValidator;

    public ValidarApiKeyServiceFilter(IApiKeyValidatorService apiKeyValidator)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Inspeção do cabeçalho customizado 'X-API-Key'
        var chaveRecebida = context.HttpContext.Request.Headers["X-API-Key"].FirstOrDefault();

        if (!_apiKeyValidator.ValidarChave(chaveRecebida))
        {
            // Curto-circuito: define o resultado imediatamente e não chama next()
            context.Result = new UnauthorizedObjectResult(new
            {
                Status = StatusCodes.Status401Unauthorized,
                Erro = "Acesso Não Autorizado",
                Mensagem = "Acesso bloqueado pelo ValidarApiKeyServiceFilter. O cabeçalho 'X-API-Key' é ausente ou inválido.",
                ChaveEsperadaParaTeste = _apiKeyValidator.ChavePadraoEsperada,
                Filtro = nameof(ValidarApiKeyServiceFilter)
            });
            return;
        }

        // Caso a chave seja válida, prossegue o pipeline executando a Action
        await next();
    }
}
