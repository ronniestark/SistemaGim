using SistemaGim.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemaGim.Data
{
    public class SistemaGimDbContext : DbContext
    {
        public DbSet<ConfiguracionPrecio> ConfiguracionPrecios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=SistemaGim;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API para reglas de negocio estrictas y borrado en cascada
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Asistencias)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}