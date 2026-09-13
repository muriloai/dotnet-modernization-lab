using System.ComponentModel.DataAnnotations;

namespace RestApisDemo.Models;

/// <summary>
/// Modelo de entrada para atualização parcial (PATCH) do preço de um produto.
/// </summary>
public sealed record AtualizarPrecoRequest(
    [Range(0.01, 1000000.00, ErrorMessage = "O novo preço deve ser maior que zero.")]
    decimal NovoPreco
);
