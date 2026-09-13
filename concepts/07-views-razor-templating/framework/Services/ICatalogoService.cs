using System.Collections.Generic;
using ViewsAndRazorDemo.Models;

namespace ViewsAndRazorDemo.Services
{
    public interface ICatalogoService
    {
        IReadOnlyList<ProdutoViewModel> ObterTodos();
        ProdutoViewModel ObterPorId(int id);
        void Adicionar(ProdutoViewModel produto);
        ResumoCatalogoViewModel ObterResumo();
        string ObterMensagemDestaque();
    }
}
