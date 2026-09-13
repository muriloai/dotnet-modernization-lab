namespace DependencyInjectionDemo.Services
{
    public class EmailNotificacaoService : INotificacaoService
    {
        public string Canal => "E-mail (Legado)";

        public string Enviar(string destinatario, string mensagem)
        {
            return $"Notificação enviada por E-mail para '{destinatario}': {mensagem}";
        }
    }
}
