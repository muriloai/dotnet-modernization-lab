using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using CachingDemo.Services;

namespace CachingDemo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly CatalogoLegadoService _service = new CatalogoLegadoService();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.TotalAcessosBanco = _service.TotalAcessosBanco;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestarStampedeLegado()
        {
            _service.LimparCache("legado_cat_eletronicos");
            _service.ResetarContadorBanco();

            int totalRequisicoes = 8;
            var sw = Stopwatch.StartNew();

            Parallel.For(0, totalRequisicoes, i =>
            {
                _service.ObterPorCategoriaSemProtecao("eletronicos");
            });

            sw.Stop();

            ViewBag.RequisicoesDisparadas = totalRequisicoes;
            ViewBag.AcessosReaisBanco = _service.TotalAcessosBanco;
            ViewBag.TempoTotalMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            ViewBag.SucessoStampede = true;

            return View("Index");
        }

        [HttpGet]
        [OutputCache(Duration = 30, VaryByParam = "categoria")]
        public ActionResult Catalogo(string categoria = "eletronicos")
        {
            ViewBag.Categoria = categoria;
            ViewBag.CarimboServidor = DateTime.Now.ToString("HH:mm:ss.fff");
            var produtos = _service.ObterPorCategoriaSemProtecao(categoria);
            return View(produtos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LimparOutputCache(string caminho)
        {
            string urlParaRemover = string.IsNullOrEmpty(caminho) ? "/Home/Catalogo?categoria=eletronicos" : caminho;
            
            try
            {
                // No ASP.NET MVC legado, a remoção dependia do caminho exato da URL
                Response.RemoveOutputCacheItem(urlParaRemover);
                ViewBag.MensagemRemocao = "Item de OutputCache removido via RemoveOutputCacheItem para o caminho: " + urlParaRemover;
            }
            catch (Exception ex)
            {
                ViewBag.MensagemRemocao = "Falha ao remover item do OutputCache: " + ex.Message;
            }

            return View("Index");
        }
    }
}
