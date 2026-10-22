using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ProcedimientosPolicialesFinMap : IEntityTypeConfiguration<procedimientos_policiales_fin>
    {
        public void Configure(EntityTypeBuilder<procedimientos_policiales_fin> builder)
        {
            builder.ToTable("PROCEDIMIENTOS_POLICIALES_FIN").HasKey(a => a.ID);
        }
    }
}
