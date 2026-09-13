using Microsoft.AspNetCore.Mvc;
using ViewsAndRazorDemo.Models;
using ViewsAndRazorDemo.Services;

namespace ViewsAndRazorDemo.Controllers;

public class ProdutosController : Controller
{
    private readonly ICatalogoService _catalogoService;

    public ProdutosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var produtos = _catalogoService.ObterTodos();
        return View(produtos);
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View(new ProdutoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(ProdutoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _catalogoService.Adicionar(model);
        return RedirectToAction(nameof(Index));
    }
}
