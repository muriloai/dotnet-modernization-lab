using System.Web.Mvc;
using System.Web.Security;

namespace AuthenticationDemo.Controllers
{
    /// <summary>
    /// Controller principal protegida por [Authorize] no ASP.NET MVC 5.
    /// Demonstra como as informações do IPrincipal e do ticket legado eram lidas.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            string cookieCriptografado = authCookie != null ? authCookie.Value : "Não encontrado";
            string dadosUserData = "Não decodificado";
            string dataExpiracao = "Não disponível";

            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                try
                {
                    var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (ticket != null)
                    {
                        dadosUserData = ticket.UserData;
                        dataExpiracao = ticket.Expiration.ToString("dd/MM/yyyy HH:mm:ss");
                    }
                }
                catch
                {
                    dadosUserData = "Erro ao decriptografar ticket (MachineKey incompatível).";
                }
            }

            ViewBag.NomeUsuario = User.Identity.Name;
            ViewBag.TipoAutenticacao = User.Identity.AuthenticationType;
            ViewBag.IsAdmin = User.IsInRole("Administrador");
            ViewBag.IsOperador = User.IsInRole("Operador");
            ViewBag.CookieNome = FormsAuthentication.FormsCookieName;
            ViewBag.CookieValor = cookieCriptografado;
            ViewBag.UserData = dadosUserData;
            ViewBag.DataExpiracao = dataExpiracao;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public ActionResult AreaAdmin()
        {
            ViewBag.Mensagem = "Acesso exclusivo de Administrador concedido via [Authorize(Roles = 'Administrador')].";
            return View();
        }
    }
}
