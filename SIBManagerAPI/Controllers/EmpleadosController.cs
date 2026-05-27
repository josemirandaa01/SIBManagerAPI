
using SIBManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIBManagerAPI.Data;
using SIBManagerAPI.DTOs;
using SIBManagerAPI.Models;

namespace GestionEmpleadosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PagoService _pago;

    public EmpleadosController(AppDbContext db, PagoService pago)
    {
        _db = db;
        _pago = pago;
    }

    // GET api/empleados?nombre=Carlos&departamentoId=1&estado=true
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? nombre,
        [FromQuery] int? departamentoId,
        [FromQuery] bool? estado)
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

        var resultado = lista.Select(e => new
        {
            e.EmpleadoId,
            NombreCompleto = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim(),
            e.NumeroSeguroSocial,
            TipoEmpleado = e.TipoEmpleado?.Nombre,
            Departamento = e.Departamento?.Nombre,
            e.Estado,
            PagoCalculado = _pago.CalcularPago(e)
        });

        return Ok(resultado);
    }

    // GET api/empleados/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Empleados
            .Include(e => e.TipoEmpleado)
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.EmpleadoId == id);

        if (e == null) return NotFound();

        return Ok(new
        {
            e.EmpleadoId,
            e.PrimerNombre,
            e.ApellidoPaterno,
            e.NumeroSeguroSocial,
            TipoEmpleado = e.TipoEmpleado?.Nombre,
            Departamento = e.Departamento?.Nombre,
            e.Estado,
            e.SalarioSemanal,
            e.SueldoPorHora,
            e.HorasTrabajadas,
            e.VentasBrutas,
            e.TarifaComision,
            e.SalarioBase,
            PagoCalculado = _pago.CalcularPago(e)
        });
    }

    // POST api/empleados
    [HttpPost]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<IActionResult> Create(EmpleadoDto dto)
    {
        var empleado = new Empleado
        {
            PrimerNombre = dto.PrimerNombre,
            ApellidoPaterno = dto.ApellidoPaterno,
            NumeroSeguroSocial = dto.NumeroSeguroSocial,
            TipoEmpleadoId = dto.TipoEmpleadoId,
            DepartamentoId = dto.DepartamentoId,
            Estado = dto.Estado,
            SalarioSemanal = dto.SalarioSemanal,
            SueldoPorHora = dto.SueldoPorHora,
            HorasTrabajadas = dto.HorasTrabajadas,
            VentasBrutas = dto.VentasBrutas,
            TarifaComision = dto.TarifaComision,
            SalarioBase = dto.SalarioBase
        };

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = empleado.EmpleadoId }, empleado);
    }

    // PUT api/empleados/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<IActionResult> Update(int id, EmpleadoDto dto)
    {
        var empleado = await _db.Empleados
            .Include(e => e.TipoEmpleado)
            .Include(e => e.Departamento)
            .FirstOrDefaultAsync(e => e.EmpleadoId == id);

        if (empleado == null) return NotFound(new { mensaje = "Empleado no encontrado" });

        // Actualizar datos
        empleado.PrimerNombre = dto.PrimerNombre;
        empleado.ApellidoPaterno = dto.ApellidoPaterno;
        empleado.NumeroSeguroSocial = dto.NumeroSeguroSocial;
        empleado.TipoEmpleadoId = dto.TipoEmpleadoId;
        empleado.DepartamentoId = dto.DepartamentoId;
        empleado.Estado = dto.Estado;
        empleado.SalarioSemanal = dto.SalarioSemanal;
        empleado.SueldoPorHora = dto.SueldoPorHora;
        empleado.HorasTrabajadas = dto.HorasTrabajadas;
        empleado.VentasBrutas = dto.VentasBrutas;
        empleado.TarifaComision = dto.TarifaComision;
        empleado.SalarioBase = dto.SalarioBase;

        await _db.SaveChangesAsync();

        // Recargar relaciones por si cambió el tipo
        await _db.Entry(empleado).Reference(e => e.TipoEmpleado).LoadAsync();
        await _db.Entry(empleado).Reference(e => e.Departamento).LoadAsync();

        // Devolver empleado actualizado con pago recalculado
        return Ok(new
        {
            empleado.EmpleadoId,
            NombreCompleto = $"{empleado.PrimerNombre} {empleado.ApellidoPaterno}".Trim(),
            empleado.NumeroSeguroSocial,
            TipoEmpleado = empleado.TipoEmpleado?.Nombre,
            Departamento = empleado.Departamento?.Nombre,
            empleado.Estado,
            empleado.SalarioSemanal,
            empleado.SueldoPorHora,
            empleado.HorasTrabajadas,
            empleado.VentasBrutas,
            empleado.TarifaComision,
            empleado.SalarioBase,
            PagoRecalculado = _pago.CalcularPago(empleado),
            Mensaje = "Empleado actualizado y pago recalculado correctamente"
        });
    }

    // DELETE api/empleados/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado == null) return NotFound();

        // Baja lógica
        empleado.Estado = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET api/empleados/5/pago
    [HttpGet("{id}/pago")]
    public async Task<IActionResult> GetPago(int id)
    {
        var e = await _db.Empleados
            .Include(e => e.TipoEmpleado)
            .FirstOrDefaultAsync(e => e.EmpleadoId == id);

        if (e == null) return NotFound();

        return Ok(new
        {
            Empleado = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim(),
            TipoEmpleado = e.TipoEmpleado?.Nombre,
            PagoCalculado = _pago.CalcularPago(e)
        });
    }


    // GET api/empleados/reporte-semanal
    [HttpGet("reporte-semanal")]
    public async Task<IActionResult> ReporteSemanal()
    {
        var empleados = await _db.Empleados
            .Include(e => e.TipoEmpleado)
            .Include(e => e.Departamento)
            .Where(e => e.Estado == true)
            .ToListAsync();

        var reporte = new ReporteSemanalDto
        {
            FechaGeneracion = DateTime.Now,
            SemanaActual = $"{InicioSemana():dd/MM/yyyy} - {FinSemana():dd/MM/yyyy}",
            TotalEmpleados = empleados.Count,
            Empleados = empleados.Select(e => DetallarPago(e)).ToList(),
            TotalNomina = empleados.Sum(e => _pago.CalcularPago(e))
        };

        return Ok(reporte);
    }

    private EmpleadoReporteDto DetallarPago(Empleado e)
    {
        var dto = new EmpleadoReporteDto
        {
            EmpleadoId = e.EmpleadoId,
            Nombre = $"{e.PrimerNombre} {e.ApellidoPaterno}".Trim(),
            NumeroSeguroSocial = e.NumeroSeguroSocial,
            TipoEmpleado = e.TipoEmpleado?.Nombre ?? "Desconocido",
            Departamento = e.Departamento?.Nombre ?? "Sin departamento",
            PagoSemanal = _pago.CalcularPago(e)
        };

        dto.Detalle = e.TipoEmpleadoId switch
        {
            // Asalariado
            1 => new DetalleReporteDto
            {
                SalarioSemanal = e.SalarioSemanal ?? 0
            },

            // Por Horas
            2 => new DetalleReporteDto
            {
                SueldoPorHora = e.SueldoPorHora ?? 0,
                HorasTrabajadas = e.HorasTrabajadas ?? 0,
                HorasNormales = Math.Min(e.HorasTrabajadas ?? 0, 40),
                HorasExtra = Math.Max((e.HorasTrabajadas ?? 0) - 40, 0),
                PagoHorasNorm = (e.SueldoPorHora ?? 0) * Math.Min(e.HorasTrabajadas ?? 0, 40),
                PagoHorasExtra = (e.SueldoPorHora ?? 0) * 1.5m * Math.Max((e.HorasTrabajadas ?? 0) - 40, 0)
            },

            // Por Comisión
            3 => new DetalleReporteDto
            {
                VentasBrutas = e.VentasBrutas ?? 0,
                TarifaComision = e.TarifaComision ?? 0,
                TarifaPorcentaje = $"{(e.TarifaComision ?? 0) * 100}%"
            },

            // Asalariado por Comisión
            4 => new DetalleReporteDto
            {
                SalarioBase = e.SalarioBase ?? 0,
                VentasBrutas = e.VentasBrutas ?? 0,
                TarifaComision = e.TarifaComision ?? 0,
                Comision = (e.VentasBrutas ?? 0) * (e.TarifaComision ?? 0),
                Bonificacion = (e.SalarioBase ?? 0) * 0.10m,
                TarifaPorcentaje = $"{(e.TarifaComision ?? 0) * 100}%"
            },

            _ => new DetalleReporteDto()
        };

        return dto;
    }

    // GET api/empleados/tipos
    [HttpGet("tipos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTipos()
    {
        var tipos = await _db.TipoEmpleado.ToListAsync();
        return Ok(tipos);
    }

    // GET api/empleados/departamentos
    [HttpGet("departamentos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartamentos()
    {
        var deptos = await _db.Departamentos.ToListAsync();
        return Ok(deptos);
    }

    private static DateTime InicioSemana() =>
        DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    private static DateTime FinSemana() =>
        InicioSemana().AddDays(6);

}