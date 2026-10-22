using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class CondicionPolicialMap : IEntityTypeConfiguration<condicion_policial>
    {
        public void Configure(EntityTypeBuilder<condicion_policial> builder)
        {
            builder.ToTable("CONDICION_POLICIAL").HasKey(a => a.ID);
        }
    }
}
