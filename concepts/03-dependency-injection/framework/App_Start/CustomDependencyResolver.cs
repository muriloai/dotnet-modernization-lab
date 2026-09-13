using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DependencyInjectionDemo.Controllers;
using DependencyInjectionDemo.Services;

namespace DependencyInjectionDemo
{
    /// <summary>
    /// Implementação didática de System.Web.Mvc.IDependencyResolver.
    /// No .NET Framework, era necessário implementar esta interface para integrar qualquer container (Unity, Autofac, etc.)
    /// ao pipeline do ASP.NET MVC, permitindo que os Controllers fossem criados com injeção de dependências no construtor.
    /// </summary>
    public class CustomDependencyResolver : IDependencyResolver
    {
        private readonly OperacaoSingleton _singletonInstance = new OperacaoSingleton();
        private readonly INotificacaoService _notificacaoService = new EmailNotificacaoService();

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(INotificacaoService))
            {
                return _notificacaoService;
            }

            if (serviceType == typeof(IClienteService))
            {
                return new ClienteService(_notificacaoService);
            }

            if (serviceType == typeof(OperacaoTransient))
            {
                // Transient: cria nova instância a cada resolução
                return new OperacaoTransient();
            }

            if (serviceType == typeof(OperacaoSingleton))
            {
                // Singleton: retorna sempre a mesma instância pré-criada
                return _singletonInstance;
            }

            if (serviceType == typeof(DiDemoController))
            {
                // Constrói o controller injetando os serviços registrados
                return new DiDemoController(
                    new ClienteService(_notificacaoService),
                    _notificacaoService,
                    new OperacaoTransient(),
                    _singletonInstance
                );
            }

            // Para tipos não registrados, o MVC tenta instanciar por padrão
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }
}
