namespace DependencyInjectionDemo.Services;

public sealed class SmsNotificacaoService : INotificacaoService
{
    public string Canal => "SMS";

    public string Enviar(string destinatario, string mensagem)
    {
        return $"Notificação enviada por SMS para '{destinatario}': {mensagem}";
    }
}
