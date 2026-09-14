using System;

namespace SolidArchitectureDemo.Domain.Entities;

// Entidade Pura de Domínio (sem acoplamento a EF Core, banco de dados ou ASP.NET)
public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public decimal SaldoCredito { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public Cliente(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do cliente é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Email corporativo inválido.", nameof(email));
        }

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SaldoCredito = 0m;
        Ativo = true;
        CriadoEm = DateTime.Now;
    }

    public void ConcederCreditoInicial(decimal valor)
    {
        if (valor < 0)
        {
            throw new InvalidOperationException("O crédito inicial não pode ser negativo.");
        }

        SaldoCredito += valor;
    }

    public void Desativar()
    {
        Ativo = false;
    }
}
