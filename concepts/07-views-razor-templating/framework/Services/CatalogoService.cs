using System;
using System.Collections.Generic;
using System.Linq;
using ViewsAndRazorDemo.Models;

namespace ViewsAndRazorDemo.Services
{
    public class CatalogoService : ICatalogoService
    {
        private static readonly List<ProdutoViewModel> _produtos = new List<ProdutoViewModel>
        {
            new ProdutoViewModel { Id = 1, Nome = "Teclado Mecânico RGB", Categoria = "Periféricos", Preco = 350.00m, EmEstoque = true },
            new ProdutoViewModel { Id = 2, Nome = "Mouse Sem Fio 16000 DPI", Categoria = "Periféricos", Preco = 220.50m, EmEstoque = true },
            new ProdutoViewModel { Id = 3, Nome = "Monitor UltraWide 29 polegadas", Categoria = "Monitores", Preco = 1450.00m, EmEstoque = false },
            new ProdutoViewModel { Id = 4, Nome = "Cadeira Ergonômica", Categoria = "Mobiliário", Preco = 980.00m, EmEstoque = true },
            new ProdutoViewModel { Id = 5, Nome = "Headset 7.1 Surround", Categoria = "Áudio", Preco = 430.00m, EmEstoque = true }
        };

        public IReadOnlyList<ProdutoViewModel> ObterTodos()
        {
            lock (_produtos)
            {
                return _produtos.ToList();
            }
        }

        public ProdutoViewModel ObterPorId(int id)
        {
            lock (_produtos)
            {
                return _produtos.FirstOrDefault(p => p.Id == id);
            }
        }

        public void Adicionar(ProdutoViewModel produto)
        {
            lock (_produtos)
            {
                var novoId = _produtos.Count > 0 ? _produtos.Max(p => p.Id) + 1 : 1;
                produto.Id = novoId;
                _produtos.Add(produto);
            }
        }

        public ResumoCatalogoViewModel ObterResumo()
        {
            lock (_produtos)
            {
                return new ResumoCatalogoViewModel
                {
                    TotalProdutos = _produtos.Count,
                    ValorTotalEstoque = _produtos.Where(p => p.EmEstoque).Sum(p => p.Preco),
                    TotalCategoriasDistintas = _produtos.Select(p => p.Categoria).Distinct().Count(),
                    ItensEmEstoque = _produtos.Count(p => p.EmEstoque),
                    CalculadoEm = DateTime.Now
                };
            }
        }

        public string ObterMensagemDestaque()
        {
            return "Catálogo servido pelo pipeline legado do ASP.NET MVC 5 no IIS.";
        }
    }
}
