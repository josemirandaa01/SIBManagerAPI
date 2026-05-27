namespace SIBManagerAPI.Models;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Rol? Rol { get; set; }
}