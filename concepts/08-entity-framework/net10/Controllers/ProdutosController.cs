using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkDemo.Data;
using EntityFrameworkDemo.Models;

namespace EntityFrameworkDemo.Controllers;

public class ProdutosController : Controller
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Demonstração de AsNoTracking() e AsSplitQuery() no EF Core
        var produtos = await _context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .AsSplitQuery()
            .OrderBy(p => p.Categoria!.Nome)
            .ThenBy(p => p.Nome)
            .ToListAsync();

        return View(produtos);
    }

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        ViewBag.Categorias = new SelectList(await _context.Categorias.AsNoTracking().ToListAsync(), "Id", "Nome");
        return View(new Produto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(Produto produto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = new SelectList(await _context.Categorias.AsNoTracking().ToListAsync(), "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        produto.AtualizadoEm = DateTime.UtcNow;
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Produto '{produto.Nome}' cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // Demonstração de Inserção em Lote com Batching Automático do EF Core
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InserirEmLote()
    {
        var cronometro = Stopwatch.StartNew();

        var categoriaHardware = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Hardware")
                                ?? await _context.Categorias.FirstAsync();

        var lote = new List<Produto>();
        for (int i = 1; i <= 5; i++)
        {
            lote.Add(new Produto
            {
                Nome = $"Componente Teste Lote #{i} ({Guid.NewGuid().ToString()[..6]})",
                Preco = 150.00m + (i * 25),
                Estoque = 10 * i,
                CategoriaId = categoriaHardware.Id,
                Ativo = true,
                AtualizadoEm = DateTime.UtcNow
            });
        }

        await _context.Produtos.AddRangeAsync(lote);
        // O EF Core empacota todas as 5 inserções em um lote único de comandos SQL
        var linhasAfetadas = await _context.SaveChangesAsync();

        cronometro.Stop();

        TempData["OperacaoTitulo"] = "Inserção em Lote (Batching Automático)";
        TempData["OperacaoDescricao"] = $"Foram inseridos {linhasAfetadas} registros em lote. O EF Core enviou os comandos em uma única viagem de rede ao banco de dados.";
        TempData["OperacaoSql"] = "INSERT INTO Produtos (...) VALUES (...), (...), (...), (...), (...)";
        TempData["OperacaoTempo"] = cronometro.ElapsedMilliseconds;

        return RedirectToAction(nameof(Index));
    }

    // Demonstração de Atualização em Massa Direta via ExecuteUpdateAsync (Sem carregar na memória)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReajustarPrecosEmMassa()
    {
        var cronometro = Stopwatch.StartNew();

        // O EF Core 10 executa o UPDATE diretamente no banco de dados.
        // Nenhuma entidade é instanciada na memória RAM e o Change Tracker não é acionado.
        var linhasAfetadas = await _context.Produtos
            .Where(p => p.Ativo)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Preco, p => p.Preco * 1.10m)
                .SetProperty(p => p.AtualizadoEm, DateTime.UtcNow));

        cronometro.Stop();

        TempData["OperacaoTitulo"] = "Atualização em Massa Direta (ExecuteUpdateAsync)";
        TempData["OperacaoDescricao"] = $"Preços reajustados em 10% para {linhasAfetadas} produtos ativos diretamente no banco, sem alocação em memória.";
        TempData["OperacaoSql"] = "UPDATE Produtos SET Preco = Preco * 1.10, AtualizadoEm = CURRENT_TIMESTAMP WHERE Ativo = 1";
        TempData["OperacaoTempo"] = cronometro.ElapsedMilliseconds;

        return RedirectToAction(nameof(Index));
    }

    // Demonstração de Exclusão em Massa Direta via ExecuteDeleteAsync
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverInativosEmMassa()
    {
        var cronometro = Stopwatch.StartNew();

        // Executa o DELETE diretamente no banco sem carregamento de entidades
        var linhasAfetadas = await _context.Produtos
            .Where(p => !p.Ativo)
            .ExecuteDeleteAsync();

        cronometro.Stop();

        TempData["OperacaoTitulo"] = "Exclusão em Massa Direta (ExecuteDeleteAsync)";
        TempData["OperacaoDescricao"] = $"Foram removidos {linhasAfetadas} produtos inativos diretamente no banco de dados com uma única consulta SQL.";
        TempData["OperacaoSql"] = "DELETE FROM Produtos WHERE Ativo = 0";
        TempData["OperacaoTempo"] = cronometro.ElapsedMilliseconds;

        return RedirectToAction(nameof(Index));
    }

    // Reinicializa a base com dados padrão para permitir novos testes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetarBanco()
    {
        await _context.Database.EnsureDeletedAsync();
        await DbInitializer.InicializarAsync(HttpContext.RequestServices);

        TempData["Sucesso"] = "Banco de dados SQLite restaurado para o estado inicial com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
