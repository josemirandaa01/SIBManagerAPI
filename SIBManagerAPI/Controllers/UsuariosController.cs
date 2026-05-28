using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIBManagerAPI.Data;
using SIBManagerAPI.Models;
using SIBManagerAPI.DTOs;
using SIBManagerAPI.Services;

namespace SIBManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LogService   _log;

    public UsuariosController(AppDbContext db, LogService log)
    {
        _db  = db;
        _log = log;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var usuarios = await _db.Usuarios
                .Include(u => u.Rol)
                .Select(u => new {
                    u.UsuarioId, u.NombreUsuario, u.Email,
                    Rol = u.Rol!.Nombre, u.Activo, u.FechaRegistro
                }).ToListAsync();

            await _log.Guardar("GET_USUARIOS", $"Consulta de usuarios - {usuarios.Count} registros");
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            await _log.Guardar("GET_USUARIOS_ERROR", "Error al consultar usuarios", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                await _log.Guardar("GET_USUARIO_NOTFOUND", $"Usuario no encontrado - Id: {id}");
                return NotFound();
            }

            await _log.Guardar("GET_USUARIO", $"Consulta de usuario - Id: {id} - {usuario.Email}");
            return Ok(new {
                usuario.UsuarioId, usuario.NombreUsuario, usuario.Email,
                Rol = usuario.Rol!.Nombre, usuario.Activo, usuario.FechaRegistro
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("GET_USUARIO_ERROR", $"Error al consultar usuario Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioDto dto)
    {
        try
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                await _log.Guardar("CREATE_USUARIO_FALLIDO", $"Email ya registrado: {dto.Email}");
                return BadRequest(new { mensaje = "El email ya esta registrado" });
            }

            if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            {
                await _log.Guardar("CREATE_USUARIO_FALLIDO", $"Nombre de usuario ya existe: {dto.NombreUsuario}");
                return BadRequest(new { mensaje = "El nombre de usuario ya existe" });
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

            await _log.Guardar("CREATE_USUARIO", $"Usuario creado - {dto.Email} - RolId: {dto.RolId}");
            return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioId }, new {
                usuario.UsuarioId, usuario.NombreUsuario, usuario.Email, usuario.RolId
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("CREATE_USUARIO_ERROR", $"Error al crear usuario: {dto.Email}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UsuarioDto dto)
    {
        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                await _log.Guardar("UPDATE_USUARIO_NOTFOUND", $"Usuario no encontrado - Id: {id}");
                return NotFound();
            }

            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.UsuarioId != id))
            {
                await _log.Guardar("UPDATE_USUARIO_FALLIDO", $"Email ya en uso: {dto.Email}");
                return BadRequest(new { mensaje = "El email ya esta en uso" });
            }

            usuario.NombreUsuario = dto.NombreUsuario;
            usuario.Email         = dto.Email;
            usuario.RolId         = dto.RolId;

            if (!string.IsNullOrEmpty(dto.Password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _db.SaveChangesAsync();

            await _log.Guardar("UPDATE_USUARIO", $"Usuario actualizado - Id: {id} - {dto.Email}");
            return NoContent();
        }
        catch (Exception ex)
        {
            await _log.Guardar("UPDATE_USUARIO_ERROR", $"Error al actualizar usuario Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                await _log.Guardar("DELETE_USUARIO_NOTFOUND", $"Usuario no encontrado - Id: {id}");
                return NotFound();
            }

            usuario.Activo = false;
            await _db.SaveChangesAsync();

            await _log.Guardar("DELETE_USUARIO", $"Usuario desactivado - Id: {id} - {usuario.Email}");
            return NoContent();
        }
        catch (Exception ex)
        {
            await _log.Guardar("DELETE_USUARIO_ERROR", $"Error al desactivar usuario Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                await _log.Guardar("ACTIVAR_USUARIO_NOTFOUND", $"Usuario no encontrado - Id: {id}");
                return NotFound();
            }

            usuario.Activo = true;
            await _db.SaveChangesAsync();

            await _log.Guardar("ACTIVAR_USUARIO", $"Usuario activado - Id: {id} - {usuario.Email}");
            return NoContent();
        }
        catch (Exception ex)
        {
            await _log.Guardar("ACTIVAR_USUARIO_ERROR", $"Error al activar usuario Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles.ToListAsync();
        await _log.Guardar("GET_ROLES", "Consulta de roles");
        return Ok(roles);
    }
}
