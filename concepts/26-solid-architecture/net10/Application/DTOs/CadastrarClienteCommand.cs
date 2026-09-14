namespace SolidArchitectureDemo.Application.DTOs;

public record CadastrarClienteCommand(
    string Nome,
    string Email,
    decimal CreditoInicial
);
