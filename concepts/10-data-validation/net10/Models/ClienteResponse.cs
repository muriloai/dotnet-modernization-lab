namespace DataValidationDemo.Models;

/// <summary>
/// Modelo de resposta retornado em caso de cadastro realizado com sucesso.
/// </summary>
public sealed record ClienteResponse(
    int Id,
    string Nome,
    string Email,
    string Cpf,
    int Idade,
    decimal RendaMensal,
    decimal LimiteCreditoAprovado,
    bool PossuiRepresentante,
    string? NomeRepresentante,
    DateTime DataCadastro
);
