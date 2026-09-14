using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionFiltersDemo.Services
{
    /// <summary>
    /// Validador estático criado para suprir a deficiência do .NET Framework 4.8.1,
    /// onde ActionFilterAttribute não suportava injeção de dependências no construtor.
    /// </summary>
    public static class ValidadorApiKeyEstatico
    {
        public const string ChavePadraoEsperada = "CHAVE-SECRETA-LAB-16";

        public static bool Validar(IEnumerable<string> valoresCabecalho)
        {
            if (valoresCabecalho == null) return false;

            var chave = valoresCabecalho.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(chave)) return false;

            return string.Equals(chave.Trim(), ChavePadraoEsperada, StringComparison.Ordinal);
        }
    }
}
