using System;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Application.DTOs;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Email,
    decimal SaldoCredito,
    bool Ativo,
    DateTime CriadoEm
)
{
    public static ClienteResponse FromEntity(Cliente c) => new(
        c.Id,
        c.Nome,
        c.Email,
        c.SaldoCredito,
        c.Ativo,
        c.CriadoEm
    );
}
