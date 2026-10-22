using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class InvolucradosIncMap : IEntityTypeConfiguration<involucrados_inc>
    {
        public void Configure(EntityTypeBuilder<involucrados_inc> builder)
        {
            builder.ToTable("INVOLUCRADOS_INC").HasKey(a => a.ID);
        }
    }
}
