namespace SIBManagerAPI.Models;

public class TipoEmpleado
{
    public int TipoEmpleadoId { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}