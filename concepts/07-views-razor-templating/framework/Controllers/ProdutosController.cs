using System.Web.Mvc;
using ViewsAndRazorDemo.Models;
using ViewsAndRazorDemo.Services;

namespace ViewsAndRazorDemo.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly ICatalogoService _catalogoService;

        // No ASP.NET MVC 5 sem DI configurado, controllers precisavam de construtor padrão
        public ProdutosController() : this(new CatalogoService())
        {
        }

        public ProdutosController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var produtos = _catalogoService.ObterTodos();
            return View(produtos);
        }

        // Child Action clássica do ASP.NET MVC
        // Era a forma de criar componentes visuais com lógica própria no legado,
        // porém forçava a execução de todo o ciclo de vida de um controller
        [ChildActionOnly]
        public ActionResult ResumoCatalogo()
        {
            var resumo = _catalogoService.ObterResumo();
            return PartialView("_ResumoCatalogo", resumo);
        }

        [HttpGet]
        public ActionResult Criar()
        {
            return View(new ProdutoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(ProdutoViewModel model)
        {
            // Validação manual repetitiva do ModelState
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _catalogoService.Adicionar(model);
            return RedirectToAction("Index");
        }
    }
}
