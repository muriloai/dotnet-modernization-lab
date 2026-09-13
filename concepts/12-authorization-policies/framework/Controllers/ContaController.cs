using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Controllers
{
    /// <summary>
    /// Controller para login e alternância de perfis no ASP.NET MVC 5 clássico.
    /// Serializa atributos no campo UserData do FormsAuthenticationTicket.
    /// </summary>
    public class ContaController : Controller
    {
        private static readonly List<UsuarioSessao> UsuariosDisponiveis = new List<UsuarioSessao>
        {
            new UsuarioSessao { Nome = "Ana Administradora", Email = "admin@empresa.com", Perfil = "Administrador", Departamento = "Tecnologia", Idade = 42, AnosExperiencia = 12 },
            new UsuarioSessao { Nome = "Beatriz Gerente", Email = "gerente.financeiro@empresa.com", Perfil = "Gerente", Departamento = "Financeiro", Idade = 36, AnosExperiencia = 7 },
            new UsuarioSessao { Nome = "Claudio Analista", Email = "analista.financeiro@empresa.com", Perfil = "Analista", Departamento = "Financeiro", Idade = 27, AnosExperiencia = 3 },
            new UsuarioSessao { Nome = "Daniel Estagiario", Email = "estagiario.ti@empresa.com", Perfil = "Estagiario", Departamento = "Tecnologia", Idade = 17, AnosExperiencia = 0 }
        };

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SelecionarPerfil()
        {
            return View(UsuariosDisponiveis);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Autenticar(string email)
        {
            var usuario = UsuariosDisponiveis.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
            {
                return RedirectToAction("SelecionarPerfil");
            }

            // Concatenação manual de dados em string única para caber no UserData do ticket legado
            string userData = string.Format("{0};{1};{2};{3}",
                usuario.Perfil, usuario.Departamento, usuario.Idade, usuario.AnosExperiencia);

            var ticket = new FormsAuthenticationTicket(
                1,
                usuario.Nome,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                false,
                userData,
                FormsAuthentication.FormsCookiePath
            );

            string ticketCriptografado = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketCriptografado)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "Documentos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("SelecionarPerfil");
        }
    }
}
