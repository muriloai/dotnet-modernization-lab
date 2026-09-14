namespace ActionFiltersDemo.Services;

/// <summary>
/// Contrato do validador de chaves de API.
/// Demonstra a capacidade de injetar serviços desacoplados em Action Filters no .NET 10.
/// </summary>
public interface IApiKeyValidatorService
{
    bool ValidarChave(string? chaveRecebida);
    string ChavePadraoEsperada { get; }
}
