namespace ActionFiltersDemo.Services;

/// <summary>
/// Implementação do serviço de validação de chave de API.
/// </summary>
public sealed class ApiKeyValidatorService : IApiKeyValidatorService
{
    public string ChavePadraoEsperada => "CHAVE-SECRETA-LAB-16";

    public bool ValidarChave(string? chaveRecebida)
    {
        if (string.IsNullOrWhiteSpace(chaveRecebida))
        {
            return false;
        }

        return string.Equals(chaveRecebida.Trim(), ChavePadraoEsperada, StringComparison.Ordinal);
    }
}
