using System.ComponentModel.DataAnnotations;

namespace AuthenticationDemo.Models
{
    /// <summary>
    /// Modelo de formulário para autenticação no ASP.NET MVC 5.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não possui formato válido.")]
        [Display(Name = "E-mail de Acesso")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; }

        [Display(Name = "Lembrar de mim")]
        public bool LembrarMe { get; set; }
    }
}
