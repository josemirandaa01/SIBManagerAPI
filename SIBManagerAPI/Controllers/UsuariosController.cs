using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIBManagerAPI.Data;
using SIBManagerAPI.Models;
using SIBManagerAPI.DTOs;
using BCrypt.Net;

namespace SIBManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]  // Solo admin gestiona usuarios
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/usuarios
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _db.Usuarios
            .Include(u => u.Rol)
            .Select(u => new
            {
                u.UsuarioId,
                u.NombreUsuario,
                u.Email,
                Rol = u.Rol!.Nombre,
                u.Activo,
                u.FechaRegistro
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // GET api/usuarios/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == id);

        if (usuario == null) return NotFound();

        return Ok(new
        {
            usuario.UsuarioId,
            usuario.NombreUsuario,
            usuario.Email,
            Rol = usuario.Rol!.Nombre,
            usuario.Activo,
            usuario.FechaRegistro
        });
    }

    // POST api/usuarios
    [HttpPost]
    public async Task<IActionResult> Create(UsuarioDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { mensaje = "El email ya está registrado" });

        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            return BadRequest(new { mensaje = "El nombre de usuario ya existe" });

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RolId = dto.RolId
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioId }, new
        {
            usuario.UsuarioId,
            usuario.NombreUsuario,
            usuario.Email,
            usuario.RolId
        });
    }

    // PUT api/usuarios/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UsuarioDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.UsuarioId != id))
            return BadRequest(new { mensaje = "El email ya está en uso" });

        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.Email = dto.Email;
        usuario.RolId = dto.RolId;

        // Solo actualiza password si se envía uno nuevo
        if (!string.IsNullOrEmpty(dto.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/usuarios/5  (baja lógica)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT api/usuarios/5/activar
    [HttpPut("{id}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET api/usuarios/roles
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles.ToListAsync();
        return Ok(roles);
    }
}