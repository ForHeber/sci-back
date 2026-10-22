using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class EstructuraSuperiorMap : IEntityTypeConfiguration<estructura_superior>
    {
        public void Configure(EntityTypeBuilder<estructura_superior> builder)
        {
            builder.ToTable("ESTRUCTURA_SUPERIOR");
        }
    }
}
