namespace DependencyInjectionDemo.Services;

/// <summary>
/// Contrato base para serviços de demonstração de ciclo de vida.
/// </summary>
public interface ILifetimeService
{
    Guid Id { get; }
    DateTime CriadoEm { get; }
    string CicloDeVida { get; }
}

/// <summary>
/// Serviço com ciclo de vida Transient. Uma nova instância e novo GUID a cada injeção.
/// </summary>
public interface ITransientService : ILifetimeService;

public sealed class TransientService : ITransientService
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CriadoEm { get; } = DateTime.Now;
    public string CicloDeVida => "Transient (Nova instância a cada solicitação)";
}

/// <summary>
/// Serviço com ciclo de vida Scoped. Mesma instância e mesmo GUID dentro da mesma requisição HTTP.
/// </summary>
public interface IScopedService : ILifetimeService;

public sealed class ScopedService : IScopedService
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CriadoEm { get; } = DateTime.Now;
    public string CicloDeVida => "Scoped (Mesma instância durante toda a requisição HTTP)";
}

/// <summary>
/// Serviço com ciclo de vida Singleton. Mesma instância e mesmo GUID por toda a execução da aplicação.
/// </summary>
public interface ISingletonService : ILifetimeService;

public sealed class SingletonService : ISingletonService
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CriadoEm { get; } = DateTime.Now;
    public string CicloDeVida => "Singleton (Única instância em toda a aplicação)";
}
