using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RepositoryPatternDemo.Models;
using RepositoryPatternDemo.Repositories;

namespace RepositoryPatternDemo.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // No MVC 5 legado sem DI configurado, controllers instanciavam a Unit of Work diretamente
        public PedidosController() : this(new UnitOfWork())
        {
        }

        public PedidosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Index()
        {
            // O repositório genérico exige passar expressões lambda para carregar relacionamentos
            var pedidos = _unitOfWork.Pedidos.GetAll(p => p.Cliente, p => p.Itens);
            return View(pedidos);
        }

        [HttpGet]
        public ActionResult Criar()
        {
            var clientes = _unitOfWork.Clientes.GetAll();
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(int clienteId, string descricaoItem, int quantidade, decimal precoUnitario)
        {
            if (string.IsNullOrWhiteSpace(descricaoItem) || quantidade <= 0 || precoUnitario <= 0)
            {
                var clientes = _unitOfWork.Clientes.GetAll();
                ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
                ModelState.AddModelError("", "Informe dados válidos para o item do pedido.");
                return View();
            }

            var novoPedido = new Pedido
            {
                NumeroPedido = "PED-2026-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                ClienteId = clienteId,
                Status = "Aprovado",
                ValorTotal = quantidade * precoUnitario,
                DataCriacao = DateTime.UtcNow,
                Itens = new List<ItemPedido>
                {
                    new ItemPedido
                    {
                        DescricaoProduto = descricaoItem,
                        Quantidade = quantidade,
                        PrecoUnitario = precoUnitario
                    }
                }
            };

            // Adiciona através do repositório genérico e confirma pelo Unit of Work
            _unitOfWork.Pedidos.Add(novoPedido);
            _unitOfWork.Complete();

            TempData["Sucesso"] = "Pedido " + novoPedido.NumeroPedido + " salvo via Repositório Genérico e Unit of Work.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
