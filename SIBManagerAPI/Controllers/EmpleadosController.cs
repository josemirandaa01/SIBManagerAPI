using SIBManagerAPI.Data;
using SIBManagerAPI.DTOs;
using SIBManagerAPI.Models;
using SIBManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SIBManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PagoService  _pago;
    private readonly LogService   _log;

    public EmpleadosController(AppDbContext db, PagoService pago, LogService log)
    {
        _db   = db;
        _pago = pago;
        _log  = log;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? nombre,
        [FromQuery] int?    departamentoId,
        [FromQuery] bool?   estado)
    {
        try
        {
            var query = _db.Empleados
                .Include(e => e.TipoEmpleado)
                .Include(e => e.Departamento)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(e =>
                    e.PrimerNombre!.Contains(nombre) ||
                    e.ApellidoPaterno.Contains(nombre));

            if (departamentoId.HasValue)
                query = query.Where(e => e.DepartamentoId == departamentoId);

            if (estado.HasValue)
                query = query.Where(e => e.Estado == estado);

            var lista = await query.ToListAsync();

            await _log.Guardar("GET_EMPLEADOS", $"Consulta de empleados - {lista.Count} registros encontrados");

            return Ok(lista.Select(e => new
            {
                e.EmpleadoId,
                NombreCompleto   = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim(),
                e.NumeroSeguroSocial,
                TipoEmpleado     = e.TipoEmpleado?.Nombre,
                Departamento     = e.Departamento?.Nombre,
                e.Estado,
                PagoCalculado    = _pago.CalcularPago(e)
            }));
        }
        catch (Exception ex)
        {
            await _log.Guardar("GET_EMPLEADOS_ERROR", "Error al consultar empleados", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var e = await _db.Empleados
                .Include(e => e.TipoEmpleado)
                .Include(e => e.Departamento)
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (e == null)
            {
                await _log.Guardar("GET_EMPLEADO_NOTFOUND", $"Empleado no encontrado - Id: {id}");
                return NotFound();
            }

            await _log.Guardar("GET_EMPLEADO", $"Consulta de empleado - Id: {id} - {e.PrimerNombre} {e.ApellidoPaterno}");

            return Ok(new
            {
                e.EmpleadoId,
                e.PrimerNombre,
                e.ApellidoPaterno,
                e.NumeroSeguroSocial,
                e.TipoEmpleadoId,
                TipoEmpleado     = e.TipoEmpleado?.Nombre,
                e.DepartamentoId,
                Departamento     = e.Departamento?.Nombre,
                e.Estado,
                e.SalarioSemanal,
                e.SueldoPorHora,
                e.HorasTrabajadas,
                e.VentasBrutas,
                e.TarifaComision,
                e.SalarioBase,
                PagoCalculado    = _pago.CalcularPago(e)
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("GET_EMPLEADO_ERROR", $"Error al consultar empleado Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EmpleadoDto dto)
    {
        try
        {
            var empleado = new Empleado
            {
                PrimerNombre       = dto.PrimerNombre,
                ApellidoPaterno    = dto.ApellidoPaterno,
                NumeroSeguroSocial = dto.NumeroSeguroSocial,
                TipoEmpleadoId     = dto.TipoEmpleadoId,
                DepartamentoId     = dto.DepartamentoId,
                Estado             = dto.Estado,
                SalarioSemanal     = dto.SalarioSemanal,
                SueldoPorHora      = dto.SueldoPorHora,
                HorasTrabajadas    = dto.HorasTrabajadas,
                VentasBrutas       = dto.VentasBrutas,
                TarifaComision     = dto.TarifaComision,
                SalarioBase        = dto.SalarioBase
            };

            _db.Empleados.Add(empleado);
            await _db.SaveChangesAsync();

            await _log.Guardar("CREATE_EMPLEADO",
                $"Empleado creado - {dto.PrimerNombre} {dto.ApellidoPaterno} - Cedula: {dto.NumeroSeguroSocial}");

            return CreatedAtAction(nameof(GetById), new { id = empleado.EmpleadoId }, empleado);
        }
        catch (Exception ex)
        {
            await _log.Guardar("CREATE_EMPLEADO_ERROR",
                $"Error al crear empleado - Cedula: {dto.NumeroSeguroSocial}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, EmpleadoDto dto)
    {
        try
        {
            var empleado = await _db.Empleados
                .Include(e => e.TipoEmpleado)
                .Include(e => e.Departamento)
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (empleado == null)
            {
                await _log.Guardar("UPDATE_EMPLEADO_NOTFOUND", $"Empleado no encontrado - Id: {id}");
                return NotFound(new { mensaje = "Empleado no encontrado" });
            }

            var pagoAnterior = _pago.CalcularPago(empleado);

            empleado.PrimerNombre       = dto.PrimerNombre;
            empleado.ApellidoPaterno    = dto.ApellidoPaterno;
            empleado.NumeroSeguroSocial = dto.NumeroSeguroSocial;
            empleado.TipoEmpleadoId     = dto.TipoEmpleadoId;
            empleado.DepartamentoId     = dto.DepartamentoId;
            empleado.Estado             = dto.Estado;
            empleado.SalarioSemanal     = dto.SalarioSemanal;
            empleado.SueldoPorHora      = dto.SueldoPorHora;
            empleado.HorasTrabajadas    = dto.HorasTrabajadas;
            empleado.VentasBrutas       = dto.VentasBrutas;
            empleado.TarifaComision     = dto.TarifaComision;
            empleado.SalarioBase        = dto.SalarioBase;

            await _db.SaveChangesAsync();

            await _db.Entry(empleado).Reference(e => e.TipoEmpleado).LoadAsync();
            await _db.Entry(empleado).Reference(e => e.Departamento).LoadAsync();

            var pagoNuevo = _pago.CalcularPago(empleado);

            await _log.Guardar("UPDATE_EMPLEADO",
                $"Empleado actualizado - Id: {id} - {empleado.PrimerNombre} {empleado.ApellidoPaterno}",
                $"Pago anterior: {pagoAnterior} | Pago nuevo: {pagoNuevo}");

            return Ok(new
            {
                empleado.EmpleadoId,
                NombreCompleto  = $"{empleado.PrimerNombre} {empleado.ApellidoPaterno}".Trim(),
                empleado.NumeroSeguroSocial,
                TipoEmpleado    = empleado.TipoEmpleado?.Nombre,
                Departamento    = empleado.Departamento?.Nombre,
                empleado.Estado,
                empleado.SalarioSemanal,
                empleado.SueldoPorHora,
                empleado.HorasTrabajadas,
                empleado.VentasBrutas,
                empleado.TarifaComision,
                empleado.SalarioBase,
                PagoRecalculado = pagoNuevo,
                Mensaje         = "Empleado actualizado y pago recalculado correctamente"
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("UPDATE_EMPLEADO_ERROR", $"Error al actualizar empleado Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var empleado = await _db.Empleados.FindAsync(id);
            if (empleado == null)
            {
                await _log.Guardar("DELETE_EMPLEADO_NOTFOUND", $"Empleado no encontrado - Id: {id}");
                return NotFound();
            }

            empleado.Estado = false;
            await _db.SaveChangesAsync();

            await _log.Guardar("DELETE_EMPLEADO",
                $"Empleado desactivado - Id: {id} - {empleado.PrimerNombre} {empleado.ApellidoPaterno}");

            return NoContent();
        }
        catch (Exception ex)
        {
            await _log.Guardar("DELETE_EMPLEADO_ERROR", $"Error al desactivar empleado Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}/pago")]
    public async Task<IActionResult> GetPago(int id)
    {
        try
        {
            var e = await _db.Empleados
                .Include(e => e.TipoEmpleado)
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (e == null)
            {
                await _log.Guardar("GET_PAGO_NOTFOUND", $"Empleado no encontrado - Id: {id}");
                return NotFound();
            }

            var pago = _pago.CalcularPago(e);
            await _log.Guardar("GET_PAGO",
                $"Calculo de pago - Id: {id} - {e.PrimerNombre} {e.ApellidoPaterno} - Pago: {pago}");

            return Ok(new
            {
                Empleado      = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim(),
                TipoEmpleado  = e.TipoEmpleado?.Nombre,
                PagoCalculado = pago
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("GET_PAGO_ERROR", $"Error al calcular pago Id: {id}", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("reporte-semanal")]
    public async Task<IActionResult> ReporteSemanal()
    {
        try
        {
            var empleados = await _db.Empleados
                .Include(e => e.TipoEmpleado)
                .Include(e => e.Departamento)
                .Where(e => e.Estado == true)
                .ToListAsync();

            var reporte = empleados.Select(e => DetallarPago(e)).ToList();
            var totalNomina = empleados.Sum(e => _pago.CalcularPago(e));

            await _log.Guardar("REPORTE_SEMANAL",
                $"Reporte semanal generado - {empleados.Count} empleados - Total nomina: {totalNomina}");

            return Ok(new
            {
                FechaGeneracion = DateTime.Now,
                SemanaActual    = $"{InicioSemana():dd/MM/yyyy} - {FinSemana():dd/MM/yyyy}",
                TotalEmpleados  = reporte.Count,
                TotalNomina     = totalNomina,
                Empleados       = reporte
            });
        }
        catch (Exception ex)
        {
            await _log.Guardar("REPORTE_SEMANAL_ERROR", "Error al generar reporte semanal", ex.Message);
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    [HttpGet("tipos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTipos()
    {
        var tipos = await _db.TipoEmpleado.ToListAsync();
        await _log.Guardar("GET_TIPOS", "Consulta de tipos de empleado");
        return Ok(tipos);
    }

    [HttpGet("departamentos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartamentos()
    {
        var deptos = await _db.Departamentos.ToListAsync();
        await _log.Guardar("GET_DEPARTAMENTOS", "Consulta de departamentos");
        return Ok(deptos);
    }

    private object DetallarPago(Empleado e)
    {
        var nombre = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim();

        return e.TipoEmpleadoId switch
        {
            1 => new { e.EmpleadoId, Nombre = nombre, e.NumeroSeguroSocial,
                TipoEmpleado = "Asalariado",
                Departamento = e.Departamento?.Nombre ?? "Sin departamento",
                Detalle      = new { SalarioSemanal = e.SalarioSemanal ?? 0 },
                PagoSemanal  = e.SalarioSemanal ?? 0 },

            2 => new { e.EmpleadoId, Nombre = nombre, e.NumeroSeguroSocial,
                TipoEmpleado = "PorHoras",
                Departamento = e.Departamento?.Nombre ?? "Sin departamento",
                Detalle      = new {
                    SueldoPorHora   = e.SueldoPorHora   ?? 0,
                    HorasTrabajadas = e.HorasTrabajadas  ?? 0,
                    HorasNormales   = Math.Min(e.HorasTrabajadas ?? 0, 40),
                    HorasExtra      = Math.Max((e.HorasTrabajadas ?? 0) - 40, 0), // horas trabajadas - 40 = horas extras
                    PagoHorasNorm   = (e.SueldoPorHora ?? 0) * Math.Min(e.HorasTrabajadas ?? 0, 40),
                    PagoHorasExtra  = (e.SueldoPorHora ?? 0) * 1.5m * Math.Max((e.HorasTrabajadas ?? 0) - 40, 0)
                },
                PagoSemanal = _pago.CalcularPago(e) },

            3 => new { e.EmpleadoId, Nombre = nombre, e.NumeroSeguroSocial,
                TipoEmpleado = "Comision",
                Departamento = e.Departamento?.Nombre ?? "Sin departamento",
                Detalle      = new {
                    VentasBrutas     = e.VentasBrutas   ?? 0,
                    TarifaComision   = e.TarifaComision  ?? 0,
                    TarifaPorcentaje = $"{(e.TarifaComision ?? 0) * 100}%"
                },
                PagoSemanal = _pago.CalcularPago(e) },

            4 => new { e.EmpleadoId, Nombre = nombre, e.NumeroSeguroSocial,
                TipoEmpleado = "AsalariadoComision",
                Departamento = e.Departamento?.Nombre ?? "Sin departamento",
                Detalle      = new {
                    SalarioBase      = e.SalarioBase    ?? 0,
                    VentasBrutas     = e.VentasBrutas   ?? 0,
                    TarifaComision   = e.TarifaComision  ?? 0,
                    Comision         = (e.VentasBrutas  ?? 0) * (e.TarifaComision ?? 0),
                    Bonificacion     = (e.SalarioBase   ?? 0) * 0.10m,
                    TarifaPorcentaje = $"{(e.TarifaComision ?? 0) * 100}%"
                },
                PagoSemanal = _pago.CalcularPago(e) },

            _ => new { e.EmpleadoId, Nombre = nombre, e.NumeroSeguroSocial,
                TipoEmpleado = "Desconocido",
                Departamento = e.Departamento?.Nombre ?? "Sin departamento",
                Detalle      = new { },
                PagoSemanal  = 0m }
        };
    }

    private static DateTime InicioSemana() =>
        DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    private static DateTime FinSemana() =>
        InicioSemana().AddDays(6);
}
