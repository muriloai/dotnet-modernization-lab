using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AuthorizationPoliciesDemo.Filters;
using AuthorizationPoliciesDemo.Models;

namespace AuthorizationPoliciesDemo.Controllers
{
    /// <summary>
    /// Controller que demonstra as restrições de autorização clássicas no .NET Framework 4.8.1:
    /// Papéis estáticos em [Authorize], atributos customizados rígidos e verificações procedurais manuais.
    /// </summary>
    [Authorize]
    public class DocumentosController : Controller
    {
        private static readonly List<RelatorioFinanceiro> RelatoriosDb = new List<RelatorioFinanceiro>
        {
            new RelatorioFinanceiro { Id = 1, Titulo = "Fechamento Mensal de Infraestrutura", Valor = 145000.00m, AutorEmail = "admin@empresa.com", AutorNome = "Ana Administradora", Departamento = "Tecnologia", Status = "Em Revisão", DataCriacao = DateTime.Now.AddDays(-5) },
            new RelatorioFinanceiro { Id = 2, Titulo = "Auditoria Tributária Q1", Valor = 89000.00m, AutorEmail = "gerente.financeiro@empresa.com", AutorNome = "Beatriz Gerente", Departamento = "Financeiro", Status = "Pendente", DataCriacao = DateTime.Now.AddDays(-2) },
            new RelatorioFinanceiro { Id = 3, Titulo = "Reembolso de Despesas", Valor = 2450.00m, AutorEmail = "analista.financeiro@empresa.com", AutorNome = "Claudio Analista", Departamento = "Financeiro", Status = "Aprovado", DataCriacao = DateTime.Now.AddHours(-12) }
        };

        [HttpGet]
        public ActionResult Index()
        {
            var principal = User as CustomPrincipal;
            ViewBag.UsuarioDados = principal != null ? principal.Dados : null;
            ViewBag.Relatorios = RelatoriosDb;
            return View();
        }

        // Nível 1: Exige apenas autenticação
        [HttpGet]
        public ActionResult PainelGeral()
        {
            ViewBag.Mensagem = "Acesso ao Painel Geral concedido com sucesso a qualquer usuário autenticado.";
            return View("Resultado");
        }

        // Nível 2: Papéis estáticos via atributo padrão
        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public ActionResult ApenasLideranca()
        {
            ViewBag.Mensagem = "Acesso exclusivo aos papéis de Gerente ou Administrador via [Authorize(Roles = 'Administrador,Gerente')].";
            return View("Resultado");
        }

        // Nível 3: Atributo customizado legado
        [HttpGet]
        [DepartamentoAuthorize("Financeiro")]
        public ActionResult ModuloFinanceiro()
        {
            ViewBag.Mensagem = "Acesso ao Módulo Financeiro concedido pelo atributo customizado [DepartamentoAuthorize('Financeiro')].";
            return View("Resultado");
        }

        // Nível 4: Atributo customizado de idade mínima
        [HttpGet]
        [IdadeMinimaAuthorize(18)]
        public ActionResult AssinaturaContratos()
        {
            ViewBag.Mensagem = "Permissão concedida pelo atributo customizado [IdadeMinimaAuthorize(18)].";
            return View("Resultado");
        }

        // Nível 5: No modelo legado, regras compostas exigiam checagens manuais procedurais dentro do método
        [HttpGet]
        public ActionResult AprovacoesVultosas()
        {
            var principal = User as CustomPrincipal;
            if (principal == null || principal.Dados == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Acesso Negado.");
            }

            bool ehLider = principal.IsInRole("Administrador") || principal.IsInRole("Gerente");
            bool ehFinanceiro = "Financeiro".Equals(principal.Dados.Departamento, StringComparison.OrdinalIgnoreCase);
            bool temExperiencia = principal.Dados.AnosExperiencia >= 5;

            // Verificação manual porque o MVC clássico não possuía AuthorizationPolicy composta
            if (!ehLider || !ehFinanceiro || !temExperiencia)
            {
                ViewBag.Erro = "Acesso Negado: A aprovação de despesas vultosas exige Gerência no Financeiro com 5+ anos de experiência.";
                return View("AcessoNegado");
            }

            ViewBag.Mensagem = "Aprovação de despesas vultosas concedida via validação procedural manual!";
            return View("Resultado");
        }

        // Nível 6: Autorização de recurso feita manualmente com if
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AtualizarRelatorio(int id, string novoStatus)
        {
            var relatorio = RelatoriosDb.FirstOrDefault(r => r.Id == id);
            if (relatorio == null)
            {
                return HttpNotFound();
            }

            var principal = User as CustomPrincipal;
            string emailUsuario = principal != null && principal.Dados != null ? principal.Dados.Email : string.Empty;

            // Sem Resource-Based Authorization nativo, o desenvolvedor tinha que escrever este if em cada action
            bool ehDono = relatorio.AutorEmail.Equals(emailUsuario, StringComparison.OrdinalIgnoreCase);
            bool ehAdmin = User.IsInRole("Administrador");

            if (!ehDono && !ehAdmin)
            {
                ViewBag.Erro = string.Format("Acesso Negado: Você não é o autor do relatório '{0}' e não possui privilégios de Administrador.", relatorio.Titulo);
                return View("AcessoNegado");
            }

            relatorio.Status = novoStatus ?? "Aprovado";
            TempData["Mensagem"] = string.Format("Relatório #{0} atualizado com sucesso para '{1}'.", id, relatorio.Status);
            return RedirectToAction("Index");
        }
    }
}
