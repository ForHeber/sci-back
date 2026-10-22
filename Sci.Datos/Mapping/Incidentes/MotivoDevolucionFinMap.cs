using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class MotivoDevolucionFinMap : IEntityTypeConfiguration<motivo_devolucion_fin>
    {
        public void Configure(EntityTypeBuilder<motivo_devolucion_fin> builder)
        {
            builder.ToTable("MOTIVO_DEVOLUCION_FIN").HasKey(a => a.ID);
        }
    }
}
