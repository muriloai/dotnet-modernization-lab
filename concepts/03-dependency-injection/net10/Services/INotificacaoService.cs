namespace DependencyInjectionDemo.Services;

/// <summary>
/// Contrato de notificação para demonstrar Keyed Services no .NET 10.
/// Duas implementações distintas da mesma interface são registradas com chaves ("email" e "sms").
/// </summary>
public interface INotificacaoService
{
    string Canal { get; }
    string Enviar(string destinatario, string mensagem);
}
