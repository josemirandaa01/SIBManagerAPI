namespace SIBManagerAPI.Models;

public class Departamento
{
    public int DepartamentoId { get; set; }
    public string Nombre { get; set; } = null!;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}