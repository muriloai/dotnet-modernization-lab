using System;

namespace CSharpEvolutionDemo.Models
{
    // Classe de modelo cerimonial tradicional em C# 7.3
    public class ClienteLegado : IEquatable<ClienteLegado>
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Categoria { get; set; }
        public int PontosFidelidade { get; set; }

        public ClienteLegado()
        {
        }

        public ClienteLegado(int id, string nome, string categoria, int pontosFidelidade)
        {
            Id = id;
            Nome = nome;
            Categoria = categoria;
            PontosFidelidade = pontosFidelidade;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClienteLegado);
        }

        public bool Equals(ClienteLegado other)
        {
            if (other == null) return false;
            return Id == other.Id &&
                   string.Equals(Nome, other.Nome, StringComparison.Ordinal) &&
                   string.Equals(Categoria, other.Categoria, StringComparison.Ordinal) &&
                   PontosFidelidade == other.PontosFidelidade;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (Nome != null ? Nome.GetHashCode() : 0);
                hash = hash * 23 + (Categoria != null ? Categoria.GetHashCode() : 0);
                hash = hash * 23 + PontosFidelidade.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("Cliente #{0} - {1} ({2})", Id, Nome, Categoria);
        }
    }
}
