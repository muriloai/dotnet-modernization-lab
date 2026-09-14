using System;
using System.Web.Mvc;
using ErrorHandlingDemo.Exceptions;

namespace ErrorHandlingDemo.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TestarSucesso()
        {
            ViewBag.Mensagem = "Operação executada com sucesso no ASP.NET MVC 5!";
            return View("Index");
        }

        [HttpGet]
        public ActionResult TestarRegraNegocio()
        {
            // Lanca uma excecao de regra de negocio que sera interceptada pelo HandleErrorAttribute
            throw new RegraNegocioLegadaException("ESTOQUE_ESGOTADO", "Falha de validação: estoque insuficiente para atendimento do pedido.");
        }

        [HttpGet]
        public ActionResult TestarErroInesperado()
        {
            // Lanca uma excecao grave nao tratada simulando falha de infraestrutura
            throw new InvalidOperationException("Falha crítica no provedor de banco de dados ou estado nulo.");
        }

        [HttpGet]
        public ActionResult Error()
        {
            // View padrao de erro do ASP.NET MVC para exibicao de HandleErrorInfo
            return View("~/Views/Shared/Error.cshtml");
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Mensagem = "O recurso solicitado não foi encontrado (404).";
            return View("Index");
        }
    }
}
