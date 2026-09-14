using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CSharpEvolutionDemo.Models;
using CSharpEvolutionDemo.Services;

namespace CSharpEvolutionDemo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly RegrasNegocioLegadoService _service = new RegrasNegocioLegadoService();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AvaliarPedido(string nomeCliente, string categoriaCliente, int pontosFidelidade, decimal valorTotal, int quantidadeItens, string status)
        {
            var cliente = new ClienteLegado(1, nomeCliente ?? "Mariana Silva", categoriaCliente ?? "VIP", pontosFidelidade);
            var itens = new List<ItemPedidoLegado>();
            decimal valorItem = quantidadeItens > 0 ? valorTotal / quantidadeItens : valorTotal;

            for (int i = 1; i <= quantidadeItens; i++)
            {
                itens.Add(new ItemPedidoLegado("Item #" + i, 1, valorItem));
            }

            var pedidoOriginal = new PedidoLegado(1001, cliente, itens, valorTotal, status ?? "Ativo");

            decimal desconto;
            string regra;
            _service.CalcularDesconto(pedidoOriginal, out desconto, out regra);

            // Sem expressão 'with', clone manual do objeto
            var pedidoComDesconto = pedidoOriginal.ClonarComNovoValor(Math.Max(0m, pedidoOriginal.ValorTotal - desconto));
            string reciboJson = _service.FormatarReciboLegado(pedidoOriginal, pedidoComDesconto, regra);

            ViewBag.RegraAplicada = regra;
            ViewBag.DescontoConcedido = desconto;
            ViewBag.ValorOriginal = pedidoOriginal.ValorTotal;
            ViewBag.ValorFinal = pedidoComDesconto.ValorTotal;
            ViewBag.ReciboJson = reciboJson;
            ViewBag.SucessoAvaliacao = true;

            return View("Index");
        }
    }
}
