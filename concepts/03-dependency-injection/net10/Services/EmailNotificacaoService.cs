namespace DependencyInjectionDemo.Services;

public sealed class EmailNotificacaoService : INotificacaoService
{
    public string Canal => "E-mail";

    public string Enviar(string destinatario, string mensagem)
    {
        return $"Notificação enviada por E-mail para '{destinatario}': {mensagem}";
    }
}
