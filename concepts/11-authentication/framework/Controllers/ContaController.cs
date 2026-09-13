using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AuthenticationDemo.Models;

namespace AuthenticationDemo.Controllers
{
    /// <summary>
    /// Controller responsável pelo ciclo de vida de autenticação no modelo clássico FormsAuthentication.
    /// </summary>
    public class ContaController : Controller
    {
        private static readonly List<UsuarioSimulado> UsuariosBase = new List<UsuarioSimulado>
        {
            new UsuarioSimulado
            {
                Id = 1,
                Nome = "Ana Administradora",
                Email = "admin@empresa.com",
                Senha = "admin123",
                Perfil = "Administrador",
                Departamento = "Tecnologia da Informação"
            },
            new UsuarioSimulado
            {
                Id = 2,
                Nome = "Carlos Operador",
                Email = "operador@empresa.com",
                Senha = "operador123",
                Perfil = "Operador",
                Departamento = "Atendimento ao Cliente"
            }
        };

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { Email = "admin@empresa.com" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = UsuariosBase.FirstOrDefault(u =>
                u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) &&
                u.Senha == model.Senha);

            if (usuario == null)
            {
                ModelState.AddModelError("", "E-mail ou senha inválidos.");
                return View(model);
            }

            // No .NET Framework, metadados adicionais eram serializados como string no UserData do ticket
            string dadosUsuario = string.Format("{0};{1};{2}", usuario.Perfil, usuario.Departamento, usuario.Nome);

            var ticket = new FormsAuthenticationTicket(
                1,                                       // Versão do ticket
                usuario.Email,                           // Nome da identidade
                DateTime.Now,                            // Data de emissão
                DateTime.Now.AddMinutes(30),             // Expiração
                model.LembrarMe,                         // Se o cookie deve ser persistente
                dadosUsuario,                            // Dados customizados em string
                FormsAuthentication.FormsCookiePath
            );

            // Criptografa o ticket utilizando a chave do MachineKey
            string ticketCriptografado = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketCriptografado)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath,
                Secure = FormsAuthentication.RequireSSL
            };

            if (model.LembrarMe)
            {
                cookie.Expires = ticket.Expiration;
            }

            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Revogação clássica síncrona do cookie
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
