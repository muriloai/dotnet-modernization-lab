namespace ErrorHandlingDemo.Exceptions;

public class IntegracaoExternaException : Exception
{
    public string ServicoDestino { get; }

    public IntegracaoExternaException(string servicoDestino, string mensagem)
        : base(mensagem)
    {
        ServicoDestino = servicoDestino;
    }
}
