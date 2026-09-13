using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using EntityFrameworkDemo.Data;
using EntityFrameworkDemo.Models;

namespace EntityFrameworkDemo.Controllers
{
    public class ProdutosController : Controller
    {
        // No MVC 5 legado sem DI, o controller gerenciava a instância do contexto diretamente
        private readonly AppDbContext _context = new AppDbContext();

        [HttpGet]
        public ActionResult Index()
        {
            // Consulta clássica do EF 6 com Include baseado em string ou expressão
            var produtos = _context.Produtos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Categoria.Nome)
                .ThenBy(p => p.Nome)
                .ToList();

            return View(produtos);
        }

        [HttpGet]
        public ActionResult Criar()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias.AsNoTracking().ToList(), "Id", "Nome");
            return View(new Produto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(Produto produto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(_context.Categorias.AsNoTracking().ToList(), "Id", "Nome", produto.CategoriaId);
                return View(produto);
            }

            produto.AtualizadoEm = DateTime.UtcNow;
            _context.Produtos.Add(produto);
            _context.SaveChanges();

            TempData["Sucesso"] = "Produto '" + produto.Nome + "' cadastrado com sucesso no EF 6.";
            return RedirectToAction("Index");
        }

        // Demonstração: No EF 6 não há batching nativo; cada entidade gera um comando SQL individual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InserirEmLote()
        {
            var cronometro = Stopwatch.StartNew();

            var categoriaHardware = _context.Categorias.FirstOrDefault(c => c.Nome == "Hardware")
                                   ?? _context.Categorias.First();

            for (int i = 1; i <= 5; i++)
            {
                _context.Produtos.Add(new Produto
                {
                    Nome = "Componente Teste EF6 #" + i + " (" + Guid.NewGuid().ToString().Substring(0, 6) + ")",
                    Preco = 150.00m + (i * 25),
                    Estoque = 10 * i,
                    CategoriaId = categoriaHardware.Id,
                    Ativo = true,
                    AtualizadoEm = DateTime.UtcNow
                });
            }

            // No EF 6, o SaveChanges envia 5 roundtrips separados de rede para o banco de dados
            var linhasAfetadas = _context.SaveChanges();

            cronometro.Stop();

            TempData["OperacaoTitulo"] = "Inserção sem Batching (Roundtrips Individuais do EF 6)";
            TempData["OperacaoDescricao"] = "Foram inseridos " + linhasAfetadas + " registros. O EF 6 enviou comandos INSERT individuais em múltiplas viagens de rede.";
            TempData["OperacaoSql"] = "INSERT INTO Produtos ...; (repetido individualmente para cada entidade)";
            TempData["OperacaoTempo"] = cronometro.ElapsedMilliseconds;

            return RedirectToAction("Index");
        }

        // Demonstração: No EF 6, para atualizar em massa é obrigatório carregar na memória e iterar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReajustarPrecosEmMassa()
        {
            var cronometro = Stopwatch.StartNew();

            // 1. Busca todas as entidades para a memória RAM (alocando objetos e ativando Change Tracker)
            var produtosAtivos = _context.Produtos.Where(p => p.Ativo).ToList();

            // 2. Itera manualmente em cada objeto alterando o estado
            foreach (var produto in produtosAtivos)
            {
                produto.Preco *= 1.10m;
                produto.AtualizadoEm = DateTime.UtcNow;
            }

            // 3. O SaveChanges gera um UPDATE SQL individual para cada linha alterada
            var linhasAfetadas = _context.SaveChanges();

            cronometro.Stop();

            TempData["OperacaoTitulo"] = "Atualização em Massa Legada (Carregamento na Memória + Loop)";
            TempData["OperacaoDescricao"] = "Reajustados " + linhasAfetadas + " produtos ativos. Foi necessário carregar todos na RAM e enviar updates individuais.";
            TempData["OperacaoSql"] = "UPDATE Produtos SET Preco = ... WHERE Id = 1; UPDATE Produtos ... WHERE Id = 2; ...";
            TempData["OperacaoTempo"] = cronometro.ElapsedMilliseconds;

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No modelo legado, era responsabilidade do desenvolvedor descartar o contexto
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
