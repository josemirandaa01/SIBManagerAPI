namespace SIBManagerAPI.Models;

public class Empleado
{
    public int EmpleadoId { get; set; }
    public string? PrimerNombre { get; set; }
    public string ApellidoPaterno { get; set; } = null!;
    public string NumeroSeguroSocial { get; set; } = null!;
    public int TipoEmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
    public bool Estado { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;


    public decimal? SalarioSemanal { get; set; }
    public decimal? SueldoPorHora { get; set; }
    public decimal? HorasTrabajadas { get; set; }
    public decimal? VentasBrutas { get; set; }
    public decimal? TarifaComision { get; set; }
    public decimal? SalarioBase { get; set; }

    public TipoEmpleado? TipoEmpleado { get; set; }
    public Departamento? Departamento { get; set; }
}