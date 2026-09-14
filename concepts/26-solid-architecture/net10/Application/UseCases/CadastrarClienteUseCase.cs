using System;
using System.Threading;
using System.Threading.Tasks;
using SolidArchitectureDemo.Application.DTOs;
using SolidArchitectureDemo.Application.Interfaces;
using SolidArchitectureDemo.Domain.Entities;

namespace SolidArchitectureDemo.Application.UseCases;

// Princípio da Responsabilidade Única (SRP): Este caso de uso tem apenas um motivo para mudar.
public class CadastrarClienteUseCase(
    IClienteRepository clienteRepository,
    INotificadorService notificadorService)
{
    public async Task<ClienteResponse> ExecutarAsync(
        CadastrarClienteCommand comando,
        CancellationToken cancellationToken = default)
    {
        var existente = await clienteRepository.ObterPorEmailAsync(comando.Email, cancellationToken);
        if (existente != null)
        {
            throw new InvalidOperationException($"Já existe um cliente cadastrado com o email '{comando.Email}'.");
        }

        // Cria a entidade de domínio aplicando suas regras invariantes
        var cliente = new Cliente(comando.Nome, comando.Email);

        if (comando.CreditoInicial > 0)
        {
            cliente.ConcederCreditoInicial(comando.CreditoInicial);
        }

        // Persistência através da abstração (DIP)
        await clienteRepository.SalvarAsync(cliente, cancellationToken);

        // Notificação através da abstração (OCP)
        await notificadorService.EnviarBoasVindasAsync(cliente, cancellationToken);

        return ClienteResponse.FromEntity(cliente);
    }
}
