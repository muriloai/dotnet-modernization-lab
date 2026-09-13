namespace RepositoryPatternDemo.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
