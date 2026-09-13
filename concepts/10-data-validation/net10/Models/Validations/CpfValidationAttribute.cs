using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataValidationDemo.Models.Validations;

/// <summary>
/// Atributo de validação personalizado para verificação de Cadastro de Pessoas Físicas (CPF).
/// Valida tanto a máscara de formatação quanto os dígitos verificadores pelo algoritmo oficial do módulo 11.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class CpfValidationAttribute : ValidationAttribute
{
    public CpfValidationAttribute()
        : base("O CPF informado é inválido.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            // Validações de obrigatoriedade devem ser tratadas por [Required]
            return ValidationResult.Success;
        }

        var cpfTexto = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(cpfTexto))
        {
            return ValidationResult.Success;
        }

        // Remove pontuações e caracteres não numéricos
        var somenteDigitos = Regex.Replace(cpfTexto, @"[^\d]", string.Empty);

        if (somenteDigitos.Length != 11)
        {
            return new ValidationResult(ErrorMessage ?? "O CPF deve conter exatamente 11 dígitos numéricos.");
        }

        // Rejeita sequências com todos os dígitos iguais (ex: 11111111111)
        if (TemTodosDigitosIguais(somenteDigitos))
        {
            return new ValidationResult(ErrorMessage ?? "O CPF informado não possui dígitos verificadores válidos.");
        }

        // Validação dos dois dígitos verificadores (algoritmo módulo 11)
        if (!ValidarDigitosVerificadores(somenteDigitos))
        {
            return new ValidationResult(ErrorMessage ?? "Os dígitos verificadores do CPF não conferem.");
        }

        return ValidationResult.Success;
    }

    private static bool TemTodosDigitosIguais(string valor)
    {
        char primeiroChar = valor[0];
        for (int i = 1; i < valor.Length; i++)
        {
            if (valor[i] != primeiroChar)
            {
                return false;
            }
        }
        return true;
    }

    private static bool ValidarDigitosVerificadores(string cpf)
    {
        // Cálculo do 1º dígito verificador
        int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma += (cpf[i] - '0') * multiplicadores1[i];
        }

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if ((cpf[9] - '0') != digito1)
        {
            return false;
        }

        // Cálculo do 2º dígito verificador
        int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            soma += (cpf[i] - '0') * multiplicadores2[i];
        }

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return (cpf[10] - '0') == digito2;
    }
}
