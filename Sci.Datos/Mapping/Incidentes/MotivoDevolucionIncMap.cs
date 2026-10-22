using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class MotivoDevolucionIncMap : IEntityTypeConfiguration<motivo_devolucion_inc>
    {
        public void Configure(EntityTypeBuilder<motivo_devolucion_inc> builder)
        {
            builder.ToTable("MOTIVO_DEVOLUCION_INC").HasKey(a => a.ID);
        }
    }
}
