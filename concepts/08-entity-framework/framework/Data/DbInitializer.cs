using System;
using System.Collections.Generic;
using System.Data.Entity;
using EntityFrameworkDemo.Models;

namespace EntityFrameworkDemo.Data
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            var hardware = new Categoria { Nome = "Hardware", Descricao = "Componentes internos e peças de reposição" };
            var perifericos = new Categoria { Nome = "Periféricos", Descricao = "Dispositivos de entrada e saída" };
            var monitores = new Categoria { Nome = "Monitores", Descricao = "Telas e displays de alta resolução" };

            context.Categorias.Add(hardware);
            context.Categorias.Add(perifericos);
            context.Categorias.Add(monitores);
            context.SaveChanges();

            var produtosIniciais = new List<Produto>
            {
                new Produto { Nome = "Processador Octa-Core 3.8GHz", Preco = 1850.00m, Estoque = 15, CategoriaId = hardware.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Placa de Vídeo 12GB GDDR6", Preco = 3400.00m, Estoque = 8, CategoriaId = hardware.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Memória RAM 16GB DDR5 5600MHz", Preco = 420.00m, Estoque = 30, CategoriaId = hardware.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Teclado Mecânico Compacto", Preco = 290.00m, Estoque = 25, CategoriaId = perifericos.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Mouse Ergonômico Vertical", Preco = 195.00m, Estoque = 12, CategoriaId = perifericos.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Monitor Gamer 27 polegadas 165Hz", Preco = 1350.00m, Estoque = 10, CategoriaId = monitores.Id, Ativo = true, AtualizadoEm = DateTime.UtcNow },
                new Produto { Nome = "Cabo DisplayPort 1.4 Antigo", Preco = 45.00m, Estoque = 0, CategoriaId = perifericos.Id, Ativo = false, AtualizadoEm = DateTime.UtcNow }
            };

            foreach (var p in produtosIniciais)
            {
                context.Produtos.Add(p);
            }
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
