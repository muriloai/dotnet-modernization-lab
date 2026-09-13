using System.Collections.Generic;
using System.Web.Mvc;
using ControllersAndActionsDemo.Models;

namespace ControllersAndActionsDemo.Controllers
{
    /// <summary>
    /// Controller do ASP.NET MVC 5 herdando de System.Web.Mvc.Controller.
    /// Focado em renderizar Views Razor e manipular formulários HTML.
    /// </summary>
    public class ProdutosMvcController : Controller
    {
        private static readonly List<ProdutoDto> Produtos = new List<ProdutoDto>
        {
            new ProdutoDto(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
            new ProdutoDto(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true)
        };

        // GET: ProdutosMvc
        public ActionResult Index()
        {
            return View(Produtos);
        }

        // GET: ProdutosMvc/Criar
        public ActionResult Criar()
        {
            return View(new ProdutoViewModel());
        }

        // POST: ProdutosMvc/Criar
        // No legado MVC, a verificação de ModelState.IsValid e o retorno manual da View
        // eram obrigatórios em toda ação de formulário.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(ProdutoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var novoId = Produtos.Count + 1;
            Produtos.Add(new ProdutoDto(novoId, model.Nome, model.Categoria, model.Preco, model.EmEstoque));

            return RedirectToAction("Index");
        }

        // GET: ProdutosMvc/ObterJson
        // PONTO DE ATENÇÃO HISTÓRICO:
        // No ASP.NET MVC, retornar JSON em verbos GET exigia explicitamente o parâmetro
        // JsonRequestBehavior.AllowGet. Sem ele, o MVC lançava uma exceção em tempo de execução.
        public ActionResult ObterJson()
        {
            return Json(Produtos, JsonRequestBehavior.AllowGet);
        }
    }
}
