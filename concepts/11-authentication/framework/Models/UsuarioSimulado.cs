namespace AuthenticationDemo.Models
{
    /// <summary>
    /// Modelo representativo de usuário para o laboratório clássico.
    /// </summary>
    public class UsuarioSimulado
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Perfil { get; set; }
        public string Departamento { get; set; }
    }
}
