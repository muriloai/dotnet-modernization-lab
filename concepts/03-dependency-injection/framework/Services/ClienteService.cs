using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionDemo.Services
{
    /// <summary>
    /// Implementação de IClienteService no .NET Framework 4.8.1 (C# 7.3).
    /// Sem suporte a Primary Constructors, exige declarar campos privados
    /// e atribuí-los manualmente dentro do construtor tradicional.
    /// </summary>
    public class ClienteService : IClienteService
    {
        private readonly INotificacaoService _notificacaoService;
        private static readonly List<ClienteModel> Clientes = new List<ClienteModel>
        {
            new ClienteModel(1, "Ana Silva", "ana.silva@empresa.com.br", "Premium"),
            new ClienteModel(2, "Carlos Eduardo", "carlos.eduardo@empresa.com.br", "Padrão"),
            new ClienteModel(3, "Mariana Souza", "mariana.souza@empresa.com.br", "Premium")
        };

        public ClienteService(INotificacaoService notificacaoService)
        {
            _notificacaoService = notificacaoService;
        }

        public IReadOnlyList<ClienteModel> ObterTodos()
        {
            _notificacaoService.Enviar("auditoria@empresa.com.br", "Consulta geral de clientes realizada no legado.");
            return Clientes.AsReadOnly();
        }

        public ClienteModel ObterPorId(int id)
        {
            return Clientes.FirstOrDefault(c => c.Id == id);
        }
    }
}
