using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ViewsAndRazorDemo.Models;
using ViewsAndRazorDemo.Services;

namespace ViewsAndRazorDemo.Pages.ProdutosRazorPage;

public class IndexModel : PageModel
{
    private readonly ICatalogoService _catalogoService;

    public IndexModel(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    public IReadOnlyList<ProdutoViewModel> Produtos { get; private set; } = [];

    [BindProperty]
    public ProdutoViewModel NovoProduto { get; set; } = new();

    public void OnGet()
    {
        Produtos = _catalogoService.ObterTodos();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            Produtos = _catalogoService.ObterTodos();
            return Page();
        }

        _catalogoService.Adicionar(NovoProduto);
        return RedirectToPage();
    }
}
