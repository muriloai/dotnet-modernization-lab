using System;

namespace DependencyInjectionDemo.Services
{
    public interface ILifetimeService
    {
        Guid Id { get; }
        string Tipo { get; }
    }

    public class OperacaoTransient : ILifetimeService
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Tipo => "Transient (Simulado no container legado)";
    }

    public class OperacaoSingleton : ILifetimeService
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Tipo => "Singleton (Instância única no container legado)";
    }
}
