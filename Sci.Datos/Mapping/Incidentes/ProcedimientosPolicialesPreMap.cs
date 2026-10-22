using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ProcedimientosPolicialesPreMap : IEntityTypeConfiguration<procedimientos_policiales_pre>
    {
        public void Configure(EntityTypeBuilder<procedimientos_policiales_pre> builder)
        {
            builder.ToTable("PROCEDIMIENTOS_POLICIALES_PRE").HasKey(a => a.ID);
        }
    }
}
