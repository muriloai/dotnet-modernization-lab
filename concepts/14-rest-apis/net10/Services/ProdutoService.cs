using RestApisDemo.Models;

namespace RestApisDemo.Services;

/// <summary>
/// Implementação em memória do serviço de produtos com controle de concorrência.
/// Registrado como Singleton para persistir alterações durante a execução do laboratório.
/// </summary>
public sealed class ProdutoService : IProdutoService
{
    private readonly List<Produto> _produtos = [];
    private readonly Lock _lock = new();
    private int _proximoId = 1;

    public ProdutoService()
    {
        CarregarDadosIniciais();
    }

    private void CarregarDadosIniciais()
    {
        var produtosIniciais = new List<Produto>
        {
            new()
            {
                Id = _proximoId++,
                Nome = "Notebook Corporativo Ultra",
                Sku = "NOT-1001",
                Preco = 4899.90m,
                Estoque = 15,
                Categoria = "Informática",
                Ativo = true,
                DataCadastro = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Id = _proximoId++,
                Nome = "Mouse sem Fio Ergonômico",
                Sku = "MOU-2002",
                Preco = 189.50m,
                Estoque = 45,
                Categoria = "Acessórios",
                Ativo = true,
                DataCadastro = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                Id = _proximoId++,
                Nome = "Teclado Mecânico RGB",
                Sku = "TEC-3003",
                Preco = 349.00m,
                Estoque = 20,
                Categoria = "Acessórios",
                Ativo = true,
                DataCadastro = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                Id = _proximoId++,
                Nome = "Monitor Gamer 27 Polegadas",
                Sku = "MON-4004",
                Preco = 1650.00m,
                Estoque = 8,
                Categoria = "Monitores",
                Ativo = true,
                DataCadastro = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                Id = _proximoId++,
                Nome = "Headset Profissional USB",
                Sku = "HED-5005",
                Preco = 279.90m,
                Estoque = 0,
                Categoria = "Áudio",
                Ativo = false,
                DataCadastro = DateTime.UtcNow.AddDays(-10)
            }
        };

        _produtos.AddRange(produtosIniciais);
    }

    public IEnumerable<ProdutoDto> ListarTodos(string? categoria = null, bool? apenasAtivos = null)
    {
        lock (_lock)
        {
            var query = _produtos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));
            }

            if (apenasAtivos.HasValue)
            {
                query = query.Where(p => p.Ativo == apenasAtivos.Value);
            }

            return query.Select(ConverterParaDto).ToList();
        }
    }

    public ProdutoDto? ObterPorId(int id)
    {
        lock (_lock)
        {
            var produto = _produtos.FirstOrDefault(p => p.Id == id);
            return produto is not null ? ConverterParaDto(produto) : null;
        }
    }

    public ProdutoDto? ObterPorSku(string sku)
    {
        lock (_lock)
        {
            var produto = _produtos.FirstOrDefault(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
            return produto is not null ? ConverterParaDto(produto) : null;
        }
    }

    public (bool Sucesso, ProdutoDto? Produto, string? Erro) Criar(CriarProdutoRequest request)
    {
        lock (_lock)
        {
            if (_produtos.Any(p => p.Sku.Equals(request.Sku, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, null, $"Já existe um produto cadastrado com o SKU '{request.Sku}'.");
            }

            var novoProduto = new Produto
            {
                Id = _proximoId++,
                Nome = request.Nome.Trim(),
                Sku = request.Sku.Trim().ToUpperInvariant(),
                Preco = request.Preco,
                Estoque = request.Estoque,
                Categoria = request.Categoria.Trim(),
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            _produtos.Add(novoProduto);
            return (true, ConverterParaDto(novoProduto), null);
        }
    }

    public (bool Sucesso, ProdutoDto? Produto, string? Erro) Atualizar(int id, AtualizarProdutoRequest request)
    {
        lock (_lock)
        {
            var produto = _produtos.FirstOrDefault(p => p.Id == id);
            if (produto is null)
            {
                return (false, null, $"Produto com ID {id} não foi localizado.");
            }

            produto.Nome = request.Nome.Trim();
            produto.Preco = request.Preco;
            produto.Estoque = request.Estoque;
            produto.Categoria = request.Categoria.Trim();
            produto.Ativo = request.Ativo;

            return (true, ConverterParaDto(produto), null);
        }
    }

    public (bool Sucesso, ProdutoDto? Produto, string? Erro) AtualizarPreco(int id, decimal novoPreco)
    {
        lock (_lock)
        {
            var produto = _produtos.FirstOrDefault(p => p.Id == id);
            if (produto is null)
            {
                return (false, null, $"Produto com ID {id} não foi localizado.");
            }

            if (novoPreco <= 0)
            {
                return (false, null, "O novo preço deve ser estritamente maior que zero.");
            }

            produto.Preco = novoPreco;
            return (true, ConverterParaDto(produto), null);
        }
    }

    public bool Excluir(int id)
    {
        lock (_lock)
        {
            var produto = _produtos.FirstOrDefault(p => p.Id == id);
            if (produto is null)
            {
                return false;
            }

            _produtos.Remove(produto);
            return true;
        }
    }

    private static ProdutoDto ConverterParaDto(Produto produto) => new(
        produto.Id,
        produto.Nome,
        produto.Sku,
        produto.Preco,
        produto.Estoque,
        produto.Categoria,
        produto.Ativo,
        produto.DataCadastro
    );
}
