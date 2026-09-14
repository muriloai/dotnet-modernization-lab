using System;

namespace CachingDemo.Models
{
    public record Produto(
        int Id,
        string Nome,
        string Categoria,
        decimal Preco,
        DateTime UltimaAtualizacao
    );
}
