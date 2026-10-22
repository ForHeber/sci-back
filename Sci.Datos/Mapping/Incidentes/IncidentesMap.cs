using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class IncidentesMap : IEntityTypeConfiguration<incidentes>
    {
        public void Configure(EntityTypeBuilder<incidentes> builder)
        {
            builder.ToTable("INCIDENTES").HasKey(a => a.ID);
        }
    }
}
