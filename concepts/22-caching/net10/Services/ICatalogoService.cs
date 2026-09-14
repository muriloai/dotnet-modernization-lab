using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CachingDemo.Models;

namespace CachingDemo.Services
{
    public interface ICatalogoService
    {
        int TotalAcessosBanco { get; }
        void ResetarContadorBanco();
        void LimparCacheLocal();
        Task<IReadOnlyList<Produto>> ObterPorCategoriaSemTravaAsync(string categoria, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Produto>> ObterPorCategoriaComTravaAsync(string categoria, CancellationToken cancellationToken = default);
        Task<Produto?> ObterPorIdDistribuidoAsync(int id, CancellationToken cancellationToken = default);
        Task InvalidarDistribuidoAsync(int id, CancellationToken cancellationToken = default);
        IReadOnlyList<Produto> ObterProdutosPorCategoria(string categoria);
    }
}
