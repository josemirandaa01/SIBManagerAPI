
namespace SIBManagerAPI.Models;

public class Rol
{
    public int RolId { get; set; }
    public string Nombre { get; set; } = null!;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}