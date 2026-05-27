namespace SIBManagerAPI.DTOs;

public class EmpleadoDto
{
    public string? PrimerNombre { get; set; }
    public string ApellidoPaterno { get; set; } = null!;
    public string NumeroSeguroSocial { get; set; } = null!;
    public int TipoEmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
    public bool Estado { get; set; } = true;

    public decimal? SalarioSemanal { get; set; }
    public decimal? SueldoPorHora { get; set; }
    public decimal? HorasTrabajadas { get; set; }
    public decimal? VentasBrutas { get; set; }
    public decimal? TarifaComision { get; set; }
    public decimal? SalarioBase { get; set; }
}