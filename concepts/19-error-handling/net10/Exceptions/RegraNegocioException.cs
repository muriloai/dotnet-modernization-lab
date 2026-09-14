namespace ErrorHandlingDemo.Exceptions;

public class RegraNegocioException : Exception
{
    public string CodigoRegra { get; }

    public RegraNegocioException(string codigoRegra, string mensagem)
        : base(mensagem)
    {
        CodigoRegra = codigoRegra;
    }
}
