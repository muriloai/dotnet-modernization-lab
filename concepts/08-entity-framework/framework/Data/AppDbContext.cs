using System.Data.Entity;
using EntityFrameworkDemo.Models;

namespace EntityFrameworkDemo.Data
{
    public class AppDbContext : DbContext
    {
        // No EF 6, a connection string é associada pelo nome configurado no Web.config
        public AppDbContext() : base("name=DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Descricao)
                .HasMaxLength(200);

            modelBuilder.Entity<Produto>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Produto>()
                .Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(80);

            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Produto>()
                .HasRequired(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .WillCascadeOnDelete(false);
        }
    }
}
