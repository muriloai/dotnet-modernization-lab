using System;

namespace ErrorHandlingDemo.Exceptions
{
    public class RegraNegocioLegadaException : Exception
    {
        public string CodigoErro { get; set; }

        public RegraNegocioLegadaException(string codigoErro, string mensagem)
            : base(mensagem)
        {
            CodigoErro = codigoErro;
        }
    }
}
