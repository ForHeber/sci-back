using Microsoft.EntityFrameworkCore;
using Sci.Datos.Mapping;
using Sci.Entidades;

namespace Sci.Datos
{
    public class DbContextUsuarios : DbContext
    {
        public DbSet<auditor>? Auditor { get; set; }
        
        public DbContextUsuarios(DbContextOptions<DbContextUsuarios> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new AuditorMap());
        }
    }    
}