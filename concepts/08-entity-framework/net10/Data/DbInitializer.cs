using Microsoft.EntityFrameworkCore;
using EntityFrameworkDemo.Models;

namespace EntityFrameworkDemo.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Criação automática do esquema caso não exista
        await context.Database.EnsureCreatedAsync();

        if (await context.Categorias.AnyAsync())
        {
            return;
        }

        var hardware = new Categoria { Nome = "Hardware", Descricao = "Componentes internos e peças de reposição" };
        var perifericos = new Categoria { Nome = "Periféricos", Descricao = "Dispositivos de entrada e saída" };
        var monitores = new Categoria { Nome = "Monitores", Descricao = "Telas e displays de alta resolução" };

        await context.Categorias.AddRangeAsync(hardware, perifericos, monitores);
        await context.SaveChangesAsync();

        var produtosIniciais = new List<Produto>
        {
            new Produto { Nome = "Processador Octa-Core 3.8GHz", Preco = 1850.00m, Estoque = 15, CategoriaId = hardware.Id, Ativo = true },
            new Produto { Nome = "Placa de Vídeo 12GB GDDR6", Preco = 3400.00m, Estoque = 8, CategoriaId = hardware.Id, Ativo = true },
            new Produto { Nome = "Memória RAM 16GB DDR5 5600MHz", Preco = 420.00m, Estoque = 30, CategoriaId = hardware.Id, Ativo = true },
            new Produto { Nome = "Teclado Mecânico Compacto", Preco = 290.00m, Estoque = 25, CategoriaId = perifericos.Id, Ativo = true },
            new Produto { Nome = "Mouse Ergonômico Vertical", Preco = 195.00m, Estoque = 12, CategoriaId = perifericos.Id, Ativo = true },
            new Produto { Nome = "Monitor Gamer 27 polegadas 165Hz", Preco = 1350.00m, Estoque = 10, CategoriaId = monitores.Id, Ativo = true },
            new Produto { Nome = "Cabo DisplayPort 1.4 Antigo", Preco = 45.00m, Estoque = 0, CategoriaId = perifericos.Id, Ativo = false }
        };

        await context.Produtos.AddRangeAsync(produtosIniciais);
        await context.SaveChangesAsync();
    }
}
