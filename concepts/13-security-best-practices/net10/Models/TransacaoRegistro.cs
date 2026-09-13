namespace SecurityBestPracticesDemo.Models;

/// <summary>
/// Registro imutável de transação concluída.
/// Utilizado para demonstrar a serialização segura tipada com System.Text.Json.
/// </summary>
public sealed record TransacaoRegistro(
    Guid Id,
    string ContaOrigem,
    string ContaDestino,
    decimal Valor,
    string Descricao,
    DateTime DataUtc,
    string ProtocoloSeguranca
);
