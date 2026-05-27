using SIBManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SIBManagerAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<TipoEmpleado> TipoEmpleado { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Empleado>()
          .HasOne(e => e.TipoEmpleado)
          .WithMany(t => t.Empleados)
          .HasForeignKey(e => e.TipoEmpleadoId);

        mb.Entity<Empleado>()
          .HasOne(e => e.Departamento)
          .WithMany(d => d.Empleados)
          .HasForeignKey(e => e.DepartamentoId);

        mb.Entity<Usuario>()
          .HasOne(u => u.Rol)
          .WithMany(r => r.Usuarios)
          .HasForeignKey(u => u.RolId);
    }
}