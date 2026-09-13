using System.Web.Mvc;
using DataValidationDemo.Models;

namespace DataValidationDemo.Controllers
{
    /// <summary>
    /// Controller do ASP.NET MVC 5 clássico.
    /// Demonstra a validação em formulários tradicionais com checagem manual de ModelState.IsValid.
    /// </summary>
    public class ClientesMvcController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Criar()
        {
            return View(new ClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(ClienteViewModel model)
        {
            // No ASP.NET MVC tradicional, a verificação de ModelState é manual em cada action
            if (!ModelState.IsValid)
            {
                // Devolve a mesma View com as mensagens preenchidas no ViewData.ModelState
                return View(model);
            }

            // Simulação de regra de negócio
            if (model.Email != null && model.Email.EndsWith("@bloqueado.com"))
            {
                ModelState.AddModelError("Email", "O domínio informado não é aceito pelas regras de negócio.");
                return View(model);
            }

            TempData["MensagemSucesso"] = string.Format("Cliente '{0}' cadastrado com sucesso no modelo MVC clássico!", model.Nome);
            return RedirectToAction("Index");
        }
    }
}
