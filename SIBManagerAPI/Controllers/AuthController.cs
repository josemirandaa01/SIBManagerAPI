using BCrypt.Net;
using SIBManagerAPI.Data;
using SIBManagerAPI.DTOs;
using SIBManagerAPI.Helpers;
using SIBManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace SIBManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthController(AppDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        return Ok(new
        {
            token = _jwt.GenerarToken(usuario),
            usuario = new { usuario.NombreUsuario, usuario.Email, Rol = usuario.Rol!.Nombre }
        });
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro(UsuarioDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { mensaje = "El email ya está registrado" });

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolId = dto.RolId
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Usuario creado exitosamente" });
    }
}