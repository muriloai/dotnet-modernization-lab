namespace ApiDesignDemo.Models;

// Modelo retornado pela V1 da API (basico, sem categoria ou calculo de desconto)
public record ProdutoV1Dto(
    int Id,
    string Nome,
    decimal Preco
);

// Modelo retornado pela V2 da API (enriquecido, categoria, desconto e links HATEOAS)
public record ProdutoV2Dto(
    int Id,
    string Nome,
    string Categoria,
    decimal PrecoOriginal,
    decimal PercentualDesconto,
    decimal PrecoFinal,
    IReadOnlyList<LinkHateoas> Links
);

public record CriarProdutoRequest(
    string? Nome,
    decimal? Preco,
    string? Categoria
);
