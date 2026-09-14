namespace JsonSerializationDemo.Models;

public record ProdutoCatalogo(
    int Id,
    string Nome,
    decimal Preco,
    string Categoria,
    int Estoque);
