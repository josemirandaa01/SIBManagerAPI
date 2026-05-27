namespace SIBManagerAPI.DTOs;

public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class UsuarioDto
{
    public string NombreUsuario { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int RolId { get; set; }
}