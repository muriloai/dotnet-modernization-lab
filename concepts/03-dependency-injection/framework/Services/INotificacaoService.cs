namespace DependencyInjectionDemo.Services
{
    public interface INotificacaoService
    {
        string Canal { get; }
        string Enviar(string destinatario, string mensagem);
    }
}
