using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidArchitectureDemo.Application.DTOs;
using SolidArchitectureDemo.Application.Interfaces;
using SolidArchitectureDemo.Application.UseCases;

namespace SolidArchitectureDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(
    CadastrarClienteUseCase cadastrarClienteUseCase,
    IClienteRepository clienteRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarClienteCommand comando,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await cadastrarClienteUseCase.ExecutarAsync(comando, cancellationToken);
            return CreatedAtAction(nameof(Listar), new { id = resultado.Id }, resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Mensagem = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> Listar(CancellationToken cancellationToken)
    {
        var clientes = await clienteRepository.ListarTodosAsync(cancellationToken);
        var responses = clientes.Select(ClienteResponse.FromEntity).ToList();
        return Ok(responses);
    }
}
