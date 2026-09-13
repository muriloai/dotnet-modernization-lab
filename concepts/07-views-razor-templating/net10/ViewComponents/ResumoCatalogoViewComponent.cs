using Microsoft.AspNetCore.Mvc;
using ViewsAndRazorDemo.Services;

namespace ViewsAndRazorDemo.ViewComponents;

public class ResumoCatalogoViewComponent : ViewComponent
{
    private readonly ICatalogoService _catalogoService;

    public ResumoCatalogoViewComponent(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // ViewComponents suportam execução assíncrona desacoplada de controllers
        await Task.Yield();
        var resumo = _catalogoService.ObterResumo();
        return View(resumo);
    }
}
