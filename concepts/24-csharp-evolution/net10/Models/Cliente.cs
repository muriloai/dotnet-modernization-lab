namespace CSharpEvolutionDemo.Models;

public record Cliente(
    int Id,
    string Nome,
    string Categoria,
    int PontosFidelidade
);
