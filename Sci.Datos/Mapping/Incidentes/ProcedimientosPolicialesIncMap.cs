using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ProcedimientosPolicialesIncMap : IEntityTypeConfiguration<procedimientos_policiales_inc>
    {
        public void Configure(EntityTypeBuilder<procedimientos_policiales_inc> builder)
        {
            builder.ToTable("PROCEDIMIENTOS_POLICIALES_INC").HasKey(a => a.ID);
        }
    }
}
