using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RepositoryPatternDemo.Data;
using RepositoryPatternDemo.Models;
using RepositoryPatternDemo.Repositories;
using RepositoryPatternDemo.Services;

namespace RepositoryPatternDemo.Controllers;

public class PedidosController : Controller
{
    private readonly PedidoService _pedidoService;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly AppDbContext _context;

    public PedidosController(
        PedidoService pedidoService,
        IPedidoRepository pedidoRepository,
        AppDbContext context)
    {
        _pedidoService = pedidoService;
        _pedidoRepository = pedidoRepository;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Abordagem 1: Consulta direta otimizada projetando para DTOs via PedidoService
        var pedidos = await _pedidoService.ListarResumosAsync();
        return View(pedidos);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(string id)
    {
        // Abordagem 2: Consulta através do repositório de domínio
        var pedido = await _pedidoRepository.ObterPorNumeroAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        var clientes = await _pedidoService.ObterClientesAsync();
        ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(int clienteId, string descricaoItem, int quantidade, decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(descricaoItem) || quantidade <= 0 || precoUnitario <= 0)
        {
            var clientes = await _pedidoService.ObterClientesAsync();
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
            ModelState.AddModelError("", "Informe dados válidos para o item do pedido.");
            return View();
        }

        var novoPedido = new Pedido
        {
            NumeroPedido = $"PED-2026-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            ClienteId = clienteId,
            Status = "Aprovado",
            Itens =
            [
                new ItemPedido
                {
                    DescricaoProduto = descricaoItem,
                    Quantidade = quantidade,
                    PrecoUnitario = precoUnitario
                }
            ]
        };

        // Persistência com transação atômica gerenciada pelo repositório de domínio
        await _pedidoRepository.CriarPedidoComTransacaoAsync(novoPedido);

        TempData["Sucesso"] = $"Pedido {novoPedido.NumeroPedido} registrado com sucesso através do Repositório de Domínio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetarBanco()
    {
        await _context.Database.EnsureDeletedAsync();
        await DbInitializer.InicializarAsync(HttpContext.RequestServices);

        TempData["Sucesso"] = "Banco de dados SQLite restaurado para o estado inicial.";
        return RedirectToAction(nameof(Index));
    }
}
