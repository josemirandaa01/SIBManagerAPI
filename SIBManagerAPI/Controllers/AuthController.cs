using BCrypt.Net;
using SIBManagerAPI.Data;
using SIBManagerAPI.DTOs;
using SIBManagerAPI.Helpers;
using SIBManagerAPI.Models;
using SIBManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SIBManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtHelper    _jwt;
    private readonly LogService   _log;

    public AuthController(AppDbContext db, JwtHelper jwt, LogService log)
    {
        _db  = db;
        _jwt = jwt;
        _log = log;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            await _log.Guardar("LOGIN_FALLIDO", $"Intento de login fallido para: {dto.Email}");
            return Unauthorized(new { mensaje = "Credenciales invalidas" });
        }

        await _log.Guardar("LOGIN", $"Usuario {dto.Email} inicio sesion correctamente");

        return Ok(new
        {
            token   = _jwt.GenerarToken(usuario),
            usuario = new { usuario.NombreUsuario, usuario.Email, Rol = usuario.Rol!.Nombre }
        });
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro(UsuarioDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            await _log.Guardar("REGISTRO_FALLIDO", $"Email ya registrado: {dto.Email}");
            return BadRequest(new { mensaje = "El email ya esta registrado" });
        }

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email         = dto.Email,
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolId         = dto.RolId
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        await _log.Guardar("REGISTRO", $"Nuevo usuario registrado: {dto.Email}");

        return Ok(new { mensaje = "Usuario creado exitosamente" });
    }
}
