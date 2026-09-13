using System;
using System.Collections.Generic;
using System.Linq;
using RestApisDemo.Models;

namespace RestApisDemo.Services
{
    /// <summary>
    /// Repositório em memória simulando persistência no .NET Framework 4.8.1.
    /// Como o ASP.NET Web API 2 não possuía injeção de dependência nativa,
    /// aplicações legadas frequentemente recorriam a classes estáticas ou Singletons manuais
    /// para evitar a complexidade de configurar resolvers externos como Unity ou Autofac.
    /// </summary>
    public static class ProdutoRepositoryLegado
    {
        private static readonly List<Produto> _produtos = new List<Produto>();
        private static readonly object _lock = new object();
        private static int _proximoId = 1;

        static ProdutoRepositoryLegado()
        {
            CarregarDadosIniciais();
        }

        private static void CarregarDadosIniciais()
        {
            _produtos.Add(new Produto
            {
                Id = _proximoId++,
                Nome = "Notebook Corporativo Ultra (Legado)",
                Sku = "NOT-1001",
                Preco = 4899.90m,
                Estoque = 15,
                Categoria = "Informática",
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-30)
            });

            _produtos.Add(new Produto
            {
                Id = _proximoId++,
                Nome = "Mouse sem Fio Ergonômico (Legado)",
                Sku = "MOU-2002",
                Preco = 189.50m,
                Estoque = 45,
                Categoria = "Acessórios",
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-25)
            });

            _produtos.Add(new Produto
            {
                Id = _proximoId++,
                Nome = "Teclado Mecânico RGB (Legado)",
                Sku = "TEC-3003",
                Preco = 349.00m,
                Estoque = 20,
                Categoria = "Acessórios",
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-20)
            });

            _produtos.Add(new Produto
            {
                Id = _proximoId++,
                Nome = "Monitor Gamer 27 Polegadas (Legado)",
                Sku = "MON-4004",
                Preco = 1650.00m,
                Estoque = 8,
                Categoria = "Monitores",
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-15)
            });

            _produtos.Add(new Produto
            {
                Id = _proximoId++,
                Nome = "Headset Profissional USB (Legado)",
                Sku = "HED-5005",
                Preco = 279.90m,
                Estoque = 0,
                Categoria = "Áudio",
                Ativo = false,
                DataCadastro = DateTime.Now.AddDays(-10)
            });
        }

        public static List<ProdutoModel> Listar(string categoria, bool? ativo)
        {
            lock (_lock)
            {
                var query = _produtos.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(categoria))
                {
                    query = query.Where(p => string.Equals(p.Categoria, categoria, StringComparison.OrdinalIgnoreCase));
                }

                if (ativo.HasValue)
                {
                    query = query.Where(p => p.Ativo == ativo.Value);
                }

                return query.Select(ConverterParaModel).ToList();
            }
        }

        public static ProdutoModel ObterPorId(int id)
        {
            lock (_lock)
            {
                var p = _produtos.FirstOrDefault(x => x.Id == id);
                return p != null ? ConverterParaModel(p) : null;
            }
        }

        public static bool ExisteSku(string sku)
        {
            lock (_lock)
            {
                return _produtos.Any(x => string.Equals(x.Sku, sku, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static ProdutoModel Adicionar(CriarProdutoModel model)
        {
            lock (_lock)
            {
                var p = new Produto
                {
                    Id = _proximoId++,
                    Nome = model.Nome.Trim(),
                    Sku = model.Sku.Trim().ToUpperInvariant(),
                    Preco = model.Preco,
                    Estoque = model.Estoque,
                    Categoria = model.Categoria.Trim(),
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                _produtos.Add(p);
                return ConverterParaModel(p);
            }
        }

        public static ProdutoModel Atualizar(int id, AtualizarProdutoModel model)
        {
            lock (_lock)
            {
                var p = _produtos.FirstOrDefault(x => x.Id == id);
                if (p == null) return null;

                p.Nome = model.Nome.Trim();
                p.Preco = model.Preco;
                p.Estoque = model.Estoque;
                p.Categoria = model.Categoria.Trim();
                p.Ativo = model.Ativo;

                return ConverterParaModel(p);
            }
        }

        public static ProdutoModel AtualizarPreco(int id, decimal novoPreco)
        {
            lock (_lock)
            {
                var p = _produtos.FirstOrDefault(x => x.Id == id);
                if (p == null) return null;

                p.Preco = novoPreco;
                return ConverterParaModel(p);
            }
        }

        public static bool Excluir(int id)
        {
            lock (_lock)
            {
                var p = _produtos.FirstOrDefault(x => x.Id == id);
                if (p == null) return false;

                _produtos.Remove(p);
                return true;
            }
        }

        private static ProdutoModel ConverterParaModel(Produto p)
        {
            return new ProdutoModel
            {
                Id = p.Id,
                Nome = p.Nome,
                Sku = p.Sku,
                Preco = p.Preco,
                Estoque = p.Estoque,
                Categoria = p.Categoria,
                Ativo = p.Ativo,
                DataCadastro = p.DataCadastro
            };
        }
    }
}
