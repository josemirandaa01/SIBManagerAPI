namespace SIBManagerAPI.Models;

public class Log
{
    public int LogId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Accion { get; set; } = null!;
    public string? Mensaje { get; set; }
    public string? Usuario { get; set; }
    public string? Detalle { get; set; }
}