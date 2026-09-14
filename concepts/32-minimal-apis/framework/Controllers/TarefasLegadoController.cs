using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MinimalApisDemo.Models;

namespace MinimalApisDemo.Controllers
{
    // Demonstracao de como qualquer endpoint simples no MVC/Web API legado requeria uma classe de Controller completa
    public class TarefasLegadoController : Controller
    {
        private static readonly List<TarefaLegada> _tarefas = new List<TarefaLegada>
        {
            new TarefaLegada { Id = 1, Titulo = "Migrar Web.config para appsettings.json", Concluida = true, Prioridade = "Alta", CriadaEm = DateTime.Now.AddHours(-3) },
            new TarefaLegada { Id = 2, Titulo = "Substituir WCF por gRPC ou REST Minimal APIs", Concluida = false, Prioridade = "Alta", CriadaEm = DateTime.Now.AddHours(-2) },
            new TarefaLegada { Id = 3, Titulo = "Configurar imagem Docker Chiseled para deploy", Concluida = false, Prioridade = "Media", CriadaEm = DateTime.Now.AddHours(-1) }
        };

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ObterTodas()
        {
            var ordenadas = _tarefas.OrderBy(t => t.Concluida).ThenByDescending(t => t.Id).ToList();
            return Json(ordenadas, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Criar(string titulo, string prioridade)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return Json(new { sucesso = false, mensagem = "O titulo e obrigatorio." });
            }

            var novoId = _tarefas.Count > 0 ? _tarefas.Max(t => t.Id) + 1 : 1;
            var nova = new TarefaLegada
            {
                Id = novoId,
                Titulo = titulo.Trim(),
                Prioridade = prioridade ?? "Media",
                Concluida = false,
                CriadaEm = DateTime.Now
            };
            _tarefas.Add(nova);

            return Json(new { sucesso = true, tarefa = nova });
        }

        [HttpPost]
        public ActionResult Alternar(int id)
        {
            var tarefa = _tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null)
            {
                return Json(new { sucesso = false, mensagem = "Tarefa nao encontrada." });
            }

            tarefa.Concluida = !tarefa.Concluida;
            return Json(new { sucesso = true, tarefa });
        }

        [HttpPost]
        public ActionResult Excluir(int id)
        {
            _tarefas.RemoveAll(t => t.Id == id);
            return Json(new { sucesso = true });
        }
    }
}
