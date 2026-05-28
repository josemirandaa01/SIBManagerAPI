namespace SIBManagerAPI.DTOs;

public class ReporteSemanalDto
{
    public DateTime FechaGeneracion { get; set; }
    public string SemanaActual { get; set; } = null!;
    public int TotalEmpleados { get; set; }
    public decimal TotalNomina { get; set; }
    public List<EmpleadoReporteDto> Empleados { get; set; } = new();
}

public class EmpleadoReporteDto
{
    public int EmpleadoId { get; set; }
    public string Nombre { get; set; } = null!;
    public string NumeroSeguroSocial { get; set; } = null!;
    public string TipoEmpleado { get; set; } = null!;
    public string Departamento { get; set; } = null!;
    public decimal PagoSemanal { get; set; }
    public DetalleReporteDto Detalle { get; set; } = new();
}

public class DetalleReporteDto
{
    public decimal? SalarioSemanal { get; set; }

    public decimal? SueldoPorHora { get; set; }
    public decimal? HorasTrabajadas { get; set; }
    public decimal? HorasNormales { get; set; }
    public decimal? HorasExtra { get; set; }
    public decimal? PagoHorasNorm { get; set; }
    public decimal? PagoHorasExtra { get; set; }

    public decimal? VentasBrutas { get; set; }
    public decimal? TarifaComision { get; set; }
    public string? TarifaPorcentaje { get; set; }

    public decimal? SalarioBase { get; set; }
    public decimal? Comision { get; set; }
    public decimal? Bonificacion { get; set; }
}