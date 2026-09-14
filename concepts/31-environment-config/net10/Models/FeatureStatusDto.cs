namespace EnvironmentDemo.Models;

public record FeatureItemDto(
    string Nome,
    bool Ativa,
    string Descricao
);

public record AmbienteInfoDto(
    string AmbienteNome,
    bool IsDevelopment,
    bool IsProduction,
    string AmbienteConfig,
    IReadOnlyList<FeatureItemDto> Features
);

public record ToggleFeatureRequest(
    string FeatureName,
    bool Enabled
);

public record CheckoutResponse(
    string Mensagem,
    string VersaoCheckout,
    bool DescontoAplicado,
    DateTime ProcessadoEm
);
