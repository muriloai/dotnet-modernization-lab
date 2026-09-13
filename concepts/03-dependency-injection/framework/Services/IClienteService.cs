using System.Collections.Generic;

namespace DependencyInjectionDemo.Services
{
    public class ClienteModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Categoria { get; set; }

        public ClienteModel(int id, string nome, string email, string categoria)
        {
            Id = id;
            Nome = nome;
            Email = email;
            Categoria = categoria;
        }
    }

    public interface IClienteService
    {
        IReadOnlyList<ClienteModel> ObterTodos();
        ClienteModel ObterPorId(int id);
    }
}
