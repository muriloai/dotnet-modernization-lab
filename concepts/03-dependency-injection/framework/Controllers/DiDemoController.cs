using System.Web.Mvc;
using DependencyInjectionDemo.Services;

namespace DependencyInjectionDemo.Controllers
{
    /// <summary>
    /// Controller que demonstra o funcionamento de injeção de dependências no ASP.NET MVC 5.
    /// Ilustra tanto a injeção recomendada por construtor quanto os riscos do Service Locator.
    /// </summary>
    public class DiDemoController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly INotificacaoService _notificacaoService;
        private readonly OperacaoTransient _transient1;
        private readonly OperacaoSingleton _singleton1;

        // Injeção de dependência recomendada: via construtor
        public DiDemoController(
            IClienteService clienteService,
            INotificacaoService notificacaoService,
            OperacaoTransient transient1,
            OperacaoSingleton singleton1)
        {
            _clienteService = clienteService;
            _notificacaoService = notificacaoService;
            _transient1 = transient1;
            _singleton1 = singleton1;
        }

        public ActionResult Index()
        {
            // Segunda resolução para demonstrar os identificadores (GUIDs)
            // No legado, fazer uma resolução sob demanda frequentemente levava desenvolvedores
            // a recorrer ao anti-pattern Service Locator:
            var transient2 = DependencyResolver.Current.GetService<OperacaoTransient>();
            var singleton2 = DependencyResolver.Current.GetService<OperacaoSingleton>();

            ViewBag.Clientes = _clienteService.ObterTodos();
            ViewBag.CanalNotificacao = _notificacaoService.Canal;

            ViewBag.Transient1Id = _transient1.Id;
            ViewBag.Transient2Id = transient2?.Id;
            ViewBag.TransientIguais = _transient1.Id == transient2?.Id;

            ViewBag.Singleton1Id = _singleton1.Id;
            ViewBag.Singleton2Id = singleton2?.Id;
            ViewBag.SingletonIguais = _singleton1.Id == singleton2?.Id;

            return View();
        }
    }
}
