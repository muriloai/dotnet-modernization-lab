using System;
using System.Web.Mvc;
using PedidosCqrsDemo.Models;
using PedidosCqrsDemo.Services;

namespace PedidosCqrsDemo.Controllers
{
    public class PedidosLegadoController : Controller
    {
        // Acoplamento direto com a interface monolitica que concentra 10 responsabilidades
        private readonly IPedidoService _pedidoService;

        public PedidosLegadoController()
        {
            // No legado sem DI nativa, frequentemente instanciado diretamente ou via Service Locator
            _pedidoService = new PedidoMonoliticoService();
        }

        public ActionResult Index()
        {
            var pedidos = _pedidoService.ObterTodos();
            return View(pedidos);
        }

        [HttpPost]
        public ActionResult Criar(string cliente, string produto, decimal valorUnitario, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(cliente) || string.IsNullOrWhiteSpace(produto))
            {
                return Json(new { sucesso = false, mensagem = "Cliente e produto sao obrigatorios" });
            }

            var pedido = new PedidoLegado
            {
                Cliente = cliente.Trim(),
                Produto = produto.Trim(),
                ValorUnitario = valorUnitario,
                Quantidade = quantidade
            };

            _pedidoService.InserirPedido(pedido);
            return Json(new { sucesso = true });
        }

        [HttpGet]
        public ActionResult ObterTodosJson()
        {
            var pedidos = _pedidoService.ObterTodos();
            return Json(pedidos, JsonRequestBehavior.AllowGet);
        }
    }
}
