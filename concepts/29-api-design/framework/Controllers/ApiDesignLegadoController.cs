using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApiDesignDemo.Models;

namespace ApiDesignDemo.Controllers
{
    public class ApiDesignLegadoController : Controller
    {
        private static readonly List<ProdutoLegado> _produtos = new List<ProdutoLegado>
        {
            new ProdutoLegado { Id = 1, Nome = "Monitor UltraWide 29 pol", Preco = 1299.90m },
            new ProdutoLegado { Id = 2, Nome = "Teclado Mecanico RGB", Preco = 349.50m },
            new ProdutoLegado { Id = 3, Nome = "Mouse Sem Fio Ergonomico", Preco = 189.00m }
        };

        public ActionResult Index()
        {
            return View();
        }

        // Endpoint V1 Legado: Envelope com Sucesso = true / false em HTTP 200
        [HttpGet]
        public ActionResult ObterProdutosV1()
        {
            var envelope = RespostaEnvelopeLegada<List<ProdutoLegado>>.Ok(_produtos);
            return Json(envelope, JsonRequestBehavior.AllowGet);
        }

        // Endpoint com Erro Legado: Retorna 200 OK com envelope indicando erro
        [HttpGet]
        public ActionResult ObterProdutoInexistenteV1(int id)
        {
            var item = _produtos.FirstOrDefault(p => p.Id == id);
            if (item == null)
            {
                // No legado, era muito frequente responder HTTP 200 com Sucesso: false
                var envelopeErro = RespostaEnvelopeLegada<ProdutoLegado>.Erro("Produto nao encontrado no banco de dados.", 404);
                return Json(envelopeErro, JsonRequestBehavior.AllowGet);
            }

            return Json(RespostaEnvelopeLegada<ProdutoLegado>.Ok(item), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CriarProdutoV1(string nome, decimal preco)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                var envelopeErro = RespostaEnvelopeLegada<ProdutoLegado>.Erro("O nome do produto nao pode ser nulo.", 400);
                return Json(envelopeErro);
            }

            var novo = new ProdutoLegado
            {
                Id = _produtos.Count > 0 ? _produtos.Max(p => p.Id) + 1 : 1,
                Nome = nome,
                Preco = preco
            };
            _produtos.Add(novo);

            return Json(RespostaEnvelopeLegada<ProdutoLegado>.Ok(novo));
        }
    }
}
