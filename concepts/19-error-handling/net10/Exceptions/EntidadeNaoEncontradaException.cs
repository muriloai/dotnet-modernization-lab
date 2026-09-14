namespace ErrorHandlingDemo.Exceptions;

public class EntidadeNaoEncontradaException : Exception
{
    public string Entidade { get; }
    public object Chave { get; }

    public EntidadeNaoEncontradaException(string entidade, object chave)
        : base($"O registro da entidade '{entidade}' com a chave identificadora '{chave}' não foi localizado no sistema.")
    {
        Entidade = entidade;
        Chave = chave;
    }
}
