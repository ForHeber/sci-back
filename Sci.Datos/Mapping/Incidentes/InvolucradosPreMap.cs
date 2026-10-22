using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class InvolucradosPreMap : IEntityTypeConfiguration<involucrados_pre>
    {
        public void Configure(EntityTypeBuilder<involucrados_pre> builder)
        {
            builder.ToTable("INVOLUCRADOS_PRE").HasKey(a => a.ID);
        }
    }
}
