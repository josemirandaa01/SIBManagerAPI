using SIBManagerAPI.Models;

namespace SIBManagerAPI.Services;

public class PagoService
{
    public decimal CalcularPago(Empleado e)
    {
        return e.TipoEmpleadoId switch
        {
            1 => e.SalarioSemanal ?? 0,

            2 => CalcularPagoHoras(e.SueldoPorHora ?? 0, e.HorasTrabajadas ?? 0),

            3 => (e.VentasBrutas ?? 0) * (e.TarifaComision ?? 0),

            4 => (e.VentasBrutas ?? 0) * (e.TarifaComision ?? 0)
               + (e.SalarioBase ?? 0)
               + (e.SalarioBase ?? 0) * 0.10m,

            _ => 0
        };
    }

    private decimal CalcularPagoHoras(decimal sueldoPorHora, decimal horasTrabajadas)
    {
        if (horasTrabajadas <= 40)
        {

            return sueldoPorHora * horasTrabajadas;
        }
        else
        {

            decimal horasNormales = sueldoPorHora * 40;
            decimal horasExtra = sueldoPorHora * 1.5m * (horasTrabajadas - 40);
            return horasNormales + horasExtra;
        }
    }
}