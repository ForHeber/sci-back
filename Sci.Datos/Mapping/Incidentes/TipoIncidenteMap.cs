using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoIncidenteMap : IEntityTypeConfiguration<tipo_incidente>
    {
        public void Configure(EntityTypeBuilder<tipo_incidente> builder)
        {
            builder.ToTable("TIPO_INCIDENTE").HasKey(a => a.ID);
        }
    }
}
