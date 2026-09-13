using System;
using System.Web.Mvc;
using SecurityBestPracticesDemo.Models;
using SecurityBestPracticesDemo.Services;

namespace SecurityBestPracticesDemo.Controllers
{
    /// <summary>
    /// Controller MVC demonstrando práticas clássicas de segurança:
    /// Validação manual com [ValidateAntiForgeryToken] e os riscos da serialização binária legada.
    /// </summary>
    public class SegurancaMvcController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Transferencia()
        {
            var model = new TransferenciaModel
            {
                ContaOrigem = "001-54321-0",
                ContaDestino = "237-98765-4",
                Valor = 750.00m,
                Descricao = "Pagamento de Serviços (MVC Clássico)"
            };

            return View(model);
        }

        /// <summary>
        /// Ação protegida manualmente com [ValidateAntiForgeryToken].
        /// Se o token for omitido ou adulterado, o ASP.NET MVC lança HttpAntiForgeryException.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TransferirComProtecao(TransferenciaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Transferencia", model);
            }

            ViewBag.Status = "Sucesso";
            ViewBag.Tipo = "Com Proteção Anti-CSRF ([ValidateAntiForgeryToken])";
            ViewBag.Mensagem = string.Format("Transferência de R$ {0:N2} executada com validação criptográfica do token de formulário!", model.Valor);
            ViewBag.Dados = model;

            return View("Resultado");
        }

        /// <summary>
        /// Ação vulnerável: O desenvolvedor esqueceu o atributo [ValidateAntiForgeryToken]!
        /// Qualquer site malicioso pode forjar uma requisição POST em nome do usuário autenticado.
        /// </summary>
        [HttpPost]
        public ActionResult TransferirSemProtecao(TransferenciaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Transferencia", model);
            }

            ViewBag.Status = "Atenção: Vulnerável a CSRF";
            ViewBag.Tipo = "Sem Validação de Token Anti-CSRF";
            ViewBag.Mensagem = string.Format("Transferência de R$ {0:N2} executada! O endpoint não exigiu token, estando vulnerável a ataques de Cross-Site Request Forgery.", model.Valor);
            ViewBag.Dados = model;

            return View("Resultado");
        }

        [HttpGet]
        public ActionResult TestarBinaryFormatter()
        {
            var objeto = new ObjetoSessaoLegado
            {
                IdentificadorSessao = "SESSAO-LEGADA-" + Guid.NewGuid().ToString().Substring(0, 8),
                Usuario = "usuario.legado@empresa.com",
                SaldoDisponivel = 15400.50m,
                UltimoAcesso = DateTime.Now
            };

            byte[] bytes = SerializadorLegadoService.SerializarParaBytes(objeto);
            var reconstruido = SerializadorLegadoService.DesserializarDeBytes(bytes);

            ViewBag.TamanhoBytes = bytes.Length;
            ViewBag.ObjetoOriginal = objeto;
            ViewBag.ObjetoReconstruido = reconstruido;

            return View("ResultadoBinaryFormatter");
        }
    }
}
