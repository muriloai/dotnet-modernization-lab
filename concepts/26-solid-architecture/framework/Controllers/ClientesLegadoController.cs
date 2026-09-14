using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SolidArchitectureDemo.Models;

namespace SolidArchitectureDemo.Controllers
{
    // Exemplo de "God Controller" legado: viola SRP, DIP e OCP
    public class ClientesLegadoController : Controller
    {
        // Simulação de banco de dados diretamente dentro do controller (violação grave de DIP)
        private static readonly List<ClienteAcoplado> _bancoClientes = new List<ClienteAcoplado>
        {
            new ClienteAcoplado
            {
                Nome = "Cliente Inicial Legado",
                Email = "inicial.legado@empresa.com.br",
                SaldoCredito = 100.00m
            }
        };

        [HttpGet]
        public ActionResult Index()
        {
            return View(_bancoClientes.OrderByDescending(c => c.CriadoEm).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cadastrar(string nome, string email, decimal creditoInicial = 0m)
        {
            // 1. Validação manual embutida no controller
            if (string.IsNullOrWhiteSpace(nome))
            {
                ViewBag.Erro = "Nome é obrigatório.";
                return View("Index", _bancoClientes.OrderByDescending(c => c.CriadoEm).ToList());
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                ViewBag.Erro = "Email inválido.";
                return View("Index", _bancoClientes.OrderByDescending(c => c.CriadoEm).ToList());
            }

            // 2. Regra de negócio de unicidade misturada no controller
            bool jaExiste = _bancoClientes.Any(c => string.Equals(c.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (jaExiste)
            {
                ViewBag.Erro = "Já existe um cliente com este email cadastrado.";
                return View("Index", _bancoClientes.OrderByDescending(c => c.CriadoEm).ToList());
            }

            // 3. Criação e persistência direta sem camada de repositório
            var novoCliente = new ClienteAcoplado
            {
                Nome = nome.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                SaldoCredito = Math.Max(0m, creditoInicial)
            };

            _bancoClientes.Add(novoCliente);

            // 4. Notificação síncrona acoplada dentro do controller
            EnviarEmailBoasVindasAcoplado(novoCliente.Email);

            ViewBag.Sucesso = "Cliente cadastrado com sucesso no controller legado!";
            return View("Index", _bancoClientes.OrderByDescending(c => c.CriadoEm).ToList());
        }

        private void EnviarEmailBoasVindasAcoplado(string email)
        {
            // No legado, a chamada a SmtpClient era feita diretamente no controller
            System.Diagnostics.Debug.WriteLine("Enviando e-mail acoplado para: " + email);
        }
    }
}
