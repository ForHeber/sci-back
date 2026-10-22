using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class EstadoSaludMap : IEntityTypeConfiguration<estado_salud>
    {
        public void Configure(EntityTypeBuilder<estado_salud> builder)
        {
            builder.ToTable("ESTADO_SALUD").HasKey(a => a.ID);
        }
    }
}
