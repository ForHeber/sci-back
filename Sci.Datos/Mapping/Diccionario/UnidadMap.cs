using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class UnidadMap : IEntityTypeConfiguration<unidad>
    {
        public void Configure(EntityTypeBuilder<unidad> builder)
        {
            builder.ToTable("UNIDAD");
        }
    }
}
