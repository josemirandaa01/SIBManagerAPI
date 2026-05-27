using SIBManagerAPI.Data;
using SIBManagerAPI.Models;

namespace SIBManagerAPI.Services;

public class LogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public LogService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    private string? GetUsuario() =>
        _http.HttpContext?.User?.FindFirst(
            System.Security.Claims.ClaimTypes.Email)?.Value;

    public async Task Info(string accion, string mensaje, string? detalle = null)
    {
        await Guardar(accion, mensaje, detalle);
    }

    public async Task Error(string accion, string mensaje, string? detalle = null)
    {
        await Guardar(accion, mensaje, detalle);
    }

    public async Task Warning(string accion, string mensaje, string? detalle = null)
    {
        await Guardar(accion, mensaje, detalle);
    }

    private async Task Guardar(string accion, string mensaje, string? detalle)
    {
        var log = new Log
        {
            Accion = accion,
            Mensaje = mensaje,
            Detalle = detalle,
            Usuario = GetUsuario(),
            Fecha = DateTime.Now
        };

        _db.Logs.Add(log);
        await _db.SaveChangesAsync();
    }
}