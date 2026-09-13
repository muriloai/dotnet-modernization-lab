using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RoutingDemo.Models;

namespace RoutingDemo.Controllers
{
    /// <summary>
    /// Controller do ASP.NET MVC 5 herdando de System.Web.Mvc.Controller.
    /// Retorna Views HTML através da tabela de rotas do RouteConfig.
    /// </summary>
    public class ProdutosController : Controller
    {
        private static readonly List<Produto> Produtos = new List<Produto>
        {
            new Produto(1, "Notebook Dell Inspiron", "hardware", 4500.00m, true),
            new Produto(2, "Monitor UltraWide 29", "perifericos", 1250.00m, true),
            new Produto(3, "Teclado Mecânico RGB", "perifericos", 350.00m, false),
            new Produto(4, "Licença Visual Studio Professional", "software", 2500.00m, true)
        };

        public ActionResult Index()
        {
            return View(Produtos);
        }

        public ActionResult Detalhes(int id)
        {
            var produto = Produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null)
            {
                return HttpNotFound();
            }

            return View(produto);
        }
    }
}
