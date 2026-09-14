namespace ApiDesignDemo.Models
{
    // Anti-padrao: Envelope manual onde a aplicacao devolve HTTP 200 para erros com a flag Sucesso = false
    public class RespostaEnvelopeLegada<T>
    {
        public bool Sucesso { get; set; }
        public T Dados { get; set; }
        public string MensagemErro { get; set; }
        public int CodigoRetorno { get; set; }

        public static RespostaEnvelopeLegada<T> Ok(T dados)
        {
            return new RespostaEnvelopeLegada<T>
            {
                Sucesso = true,
                Dados = dados,
                CodigoRetorno = 200
            };
        }

        public static RespostaEnvelopeLegada<T> Erro(string mensagem, int codigoRetorno = 400)
        {
            return new RespostaEnvelopeLegada<T>
            {
                Sucesso = false,
                Dados = default(T),
                MensagemErro = mensagem,
                CodigoRetorno = codigoRetorno
            };
        }
    }
}
